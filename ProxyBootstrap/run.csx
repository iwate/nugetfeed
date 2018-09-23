using System;
using System.IO;

public static void Run(TimerInfo myTimer, ILogger log)
{
    File.WriteAllText(@"D:\home\site\wwwroot\proxies.json",File.ReadAllText(@"D:\home\site\wwwroot\proxies.json"));
}
