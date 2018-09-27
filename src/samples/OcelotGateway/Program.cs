using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Examples.OcelotGateway
{
    public class Program
    {
        private static readonly string IP = "127.0.0.1";
        private static readonly string Port = "5000";

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel()
                .UseUrls($"http://{IP}:{Port}")
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    builder.AddYamlFile("appsettings.yml", false, true);
                    builder.AddYamlFile("ocelot.yml", false, true);
                });
    }
}