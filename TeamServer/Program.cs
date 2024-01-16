using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HardHatCore.TeamServer
{
    public class Program
    {
        public static IHost WebHost;
        public static void Main(string[] args)
        {
            if (args.Count() > 0)
            {
                if (args[0] != null)
                {
                    Console.WriteLine("Setting Teamserver IP to: " + args[0]);
                    Startup.TeamserverIP = args[0];
                }
                //reset args
                args = new string[] { };
            }


            WebHost = CreateHostBuilder(args).Build();
            WebHost.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(async webBuilder =>
                {
                    await CertGen.GenerateCert();
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {
                        //if Startup.TeamserverIP is not null then use that for the ip to listen on
                        if (Startup.TeamserverIP != null)
                        {
                            //split the ip address and port, then use the ip address and port to start kestrel
                            string[] ipAndPort = Startup.TeamserverIP.Split(":");
                            serverOptions.Listen(System.Net.IPAddress.Parse(ipAndPort[0]), int.Parse(ipAndPort[1]), listenOptions =>
                            {
                                listenOptions.UseHttps(new X509Certificate2(CertGen.CertificatePath, CertGen.CertificatePassword));
                            });
                        }
                        else
                        {
                            serverOptions.ConfigureEndpointDefaults(listenOptions =>
                            {
                                listenOptions.UseHttps(new X509Certificate2(CertGen.CertificatePath, CertGen.CertificatePassword));

                            });
                        }
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
