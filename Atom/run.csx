#r "Microsoft.Extensions.Configuration.Abstractions"
#r "Microsoft.Extensions.Configuration"
#r "Microsoft.Extensions.Configuration.EnvironmentVariables"

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using ODataHttpClient;
using ODataHttpClient.Models;

static IConfigurationRoot config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
static HttpClient http = new HttpClient();

public static async Task<HttpResponseMessage> Run(HttpRequest req, ILogger log)
{
    string id = req.Query["id"];

    if (string.IsNullOrEmpty(id))
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    
    log.LogInformation(new EventId(10000, "Package"), id);

    var odata = new ODataClient(http);

    string url = $"https://api.nuget.org/v3/registration3/{id.ToLower()}/index.json";
    IEnumerable<Entry> entries = Enumerable.Empty<Entry>();
    for (var i = 0; i < 3 && entries.Count() == 0 && url != null; i++) {
        var res = await odata.SendAsync(Request.Get(url));

        if (res.StatusCode == (HttpStatusCode)429)
            return new HttpResponseMessage((HttpStatusCode)429);

        if (res.StatusCode != HttpStatusCode.OK)
            return new HttpResponseMessage(HttpStatusCode.NotFound);

        entries = res.ReadAs<IEnumerable<Entry>>("$..catalogEntry")
                        .OrderByDescending(e => e.published)
                        .Take(20)
                        .ToArray();

        if (entries.Count() == 0) {
            url = res.ReadAs<IEnumerable<string>>("$.items[*]['@id']").LastOrDefault();
        }
    }

    if (entries.Count() == 0)
        return new HttpResponseMessage(HttpStatusCode.NotFound);

    var host = config["CUSTOM_HOSTNAME"] ?? config["WEBSITE_HOSTNAME"];
    var protocol = config["CUSTOM_PROTOCOL"] ?? (host.EndsWith(".azurewebsites.net") ? "https" : "http");
    
    var atom = $@"<?xml version='1.0' encoding='UTF-8'?>
<feed xmlns='http://www.w3.org/2005/Atom' xml:lang='en'>
    <id>tag:{id}.{host}</id>
    <title>{id}</title>
    <updated>{entries.First().published.DateTime:o}</updated>
    <link rel='alternate' type='text/html' href='https://www.nuget.org/packages/{id}' />
    <link rel='self' type='application/atom+xml' href='{protocol}://{host}/packages/{id}' />
    {string.Join("\n", entries.Select(e => e.ToAtom(id)))}
</feed>";

    var content = new StringContent(atom, Encoding.UTF8, "application/atom+xml");
    content.Headers.Expires = DateTimeOffset.Now.AddDays(1);
    content.Headers.LastModified = entries.First().published;    
    return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
}
public class Entry
{
    public DateTimeOffset published {get;set;}
    public string version { get; set; }
    
    public string ToAtom(string id)
    {
        return $@"<entry>
        <id>https://www.nuget.org/packages/{id}/{version}</id>
        <title>{id}#{version}</title>
        <link rel='alternate' type='text/html' href='https://www.nuget.org/packages/{id}/{version}' />
        <updated>{published.DateTime:o}</updated>
        <summary>{id} {version} was published at {published.DateTime}.</summary>
    </entry>";
    }
}
