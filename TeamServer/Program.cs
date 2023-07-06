using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TeamServer.Utilities;

namespace TeamServer
{
    public class Program
    {
        public static IHost WebHost;
        public static void Main(string[] args)
        {
            WebHost = CreateHostBuilder(args).Build();
            WebHost.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(async webBuilder =>
                {
                    await CertGen.GeneerateCert();
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        serverOptions.ConfigureEndpointDefaults(listenOptions =>
                        {
                            listenOptions.UseHttps(new X509Certificate2(CertGen.CertificatePath, CertGen.CertificatePassword));

                        });
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
