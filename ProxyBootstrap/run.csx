#r "Newtonsoft.Json"
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static void Run(TimerInfo myTimer, ILogger log)
{
    File.WriteAllText(@"D:\home\site\wwwroot\proxies.json",File.ReadAllText(@"D:\home\site\wwwroot\proxies.json"));
    var json = File.ReadAllText(@"D:\home\site\wwwroot\ProxyBootstrap\function.json");
    var o = JObject.Parse(json);
    o["disabled"] = true;
    File.WriteAllText(@"D:\home\site\wwwroot\ProxyBootstrap\function.json", o.ToString());
}
