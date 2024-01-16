using System;
using System.IO;
using System.Net.Security;
using System.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using HardHatCore.ApiModels.Aspects.ContractorSystem_InvocationPoints;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Endpoints.ConfigModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HardHatCore.TeamServer.Models
{
    public class HttpManager : Manager
    {
        public override string Name { get; set; } // properties allows us to get Name when Manager is created later so set will go with creation functions later
        public int ConnectionPort { get; set; }         // connection location for engineers created with this Manager
        public string ConnectionAddress { get; set; }   // connection location for engineers created with this Manager
        public string BindAddress { get; set; }         // local teamserver address to bind to
        public int BindPort { get; set; }               // local teamserver port to bind to 
        public bool Active => _tokenSource is not null && !_tokenSource.IsCancellationRequested;     // active is true when Manager is running which is whenever the token source is not null and not cancelled
        public bool IsSecure { get; set; }
        public string CertificatePath { get; set; }
        public string CertificatePassword { get; set; } = "p@ssw0rd";
        private X509Certificate2 cert { get; set; }

        public C2Profile c2Profile { get; set;}
        
        public override ManagerType Type { get; set; } = ManagerType.http;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        


        private HttpManager(string name, string connectionAddress,int connectionPort ,string bindAddress,int bindPort, bool isSecure, C2Profile profile)    //Constructor for HttpManager
        {
            Name = name;
            ConnectionPort = connectionPort;
            ConnectionAddress = connectionAddress;
            BindAddress = bindAddress;
            BindPort = bindPort;
            IsSecure = isSecure;
            c2Profile = profile;
        }

        public HttpManager() { }

        [FuncCallAspect(
            "OnHttpManagerCreate",
            "Fires when a new HttpManager is created. Hook on Start to get the creation args of (name, address, port, bind address, bind port, is secure, and the C2 profile). Hook the End to get the created HttpManager",
            "TeamServer"
            )
        ]
        public static HttpManager HttpManagerFactoryFunc(string name, string connectionAddress, int connectionPort, string bindAddress, int bindPort, bool isSecure, C2Profile profile)
        {
            return new HttpManager(name, connectionAddress, connectionPort, bindAddress, bindPort, isSecure, profile);
        }
        
        public override async Task Start()
        {
            try
            {
                //check that ConnectionAddress is in the format of an ip address and that ConnectionPort is between 1-65535 and if either one is not cancel the token
                if (IsSecure)
                {
                    Type = ManagerType.https;
                    await GenerateCert();

                    var hostBuilder = new HostBuilder().ConfigureWebHostDefaults(host =>
                    {
                        host.UseUrls($"https://{BindAddress}:{BindPort}");
                        host.Configure(ConfigureApp);
                        host.ConfigureServices(ConfigureServices);
                        // have the host use the certificate
                        host.ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.Limits.MaxRequestBodySize = int.MaxValue;
                            serverOptions.AddServerHeader = false;
                            serverOptions.ConfigureEndpointDefaults(listenOptions =>
                            {
                                listenOptions.UseHttps(new X509Certificate2(CertificatePath, CertificatePassword));
                                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
                            });
                            //set connectionOptions to make sure client has a cert
                            serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                            {
                                httpsOptions.SslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
                                httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                                httpsOptions.ClientCertificateValidation = (certificate, chain, sslPolicyErrors) =>
                                {
                                    //ignore remote certificate chain errors
                                    if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                                    {
                                        return true;
                                    }
                                    else if (sslPolicyErrors == SslPolicyErrors.None)
                                    {
                                        return true;
                                    }

                                    //print the errors
                                    foreach (var error in sslPolicyErrors.ToString().Split(','))
                                    {
                                        Console.WriteLine(error);
                                    }
                                    return false;
                                };
                            });
                        });
                    });

                    var host = hostBuilder.Build();
                    await host.RunAsync(_tokenSource.Token);
                }
                else
                {
                    var hostBuilder = new HostBuilder().ConfigureWebHostDefaults(host =>
                    {
                        host.UseUrls($"http://{BindAddress}:{BindPort}");
                        host.Configure(ConfigureApp);
                        host.ConfigureServices(ConfigureServices);
                        host.ConfigureKestrel(serverOptions =>
                        {
                            serverOptions.Limits.MaxRequestBodySize = int.MaxValue;
                            serverOptions.AddServerHeader = false;
                            serverOptions.ConfigureEndpointDefaults(listenOptions =>
                            {
                                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
                            });
                        });
                    });
                    var host = hostBuilder.Build();
                    await host.RunAsync(_tokenSource.Token);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }



        }


        private void ConfigureServices(IServiceCollection services)
        {
            //allows the urls the endpoint should listen on to be injected automatically
            var assetURLs = new AssetCommUrls();
            assetURLs.Urls = c2Profile.Urls.ToArray();
            services.AddSingleton(assetURLs);
            //adds fast endpoints and makes sure to filter out the ones that are not needed
            services.AddFastEndpoints(c =>
            {
                c.Filter = (x => 
                { 
                    if (x.Name.Contains("HandleAsset")) { return true; } return false; 
                });
            });
            
        }

        private void ConfigureApp(IApplicationBuilder app)
        {
            app.UseRouting();
            //List<string> urls = c2Profile.Urls;
            app.UseEndpoints(e =>
            {
                e.MapFastEndpoints(c =>
                {
                    c.Endpoints.Configurator = (x) =>
                    {
                        x.AllowAnonymous();
                    };
                });
            });
            app.UseStaticFiles();
        }

        public override void Stop()
        {
            _tokenSource.Cancel();
            //check to make sure the token is cancelled and if it is then set it to null
            if (_tokenSource.IsCancellationRequested)
            {
                _tokenSource = null;
            }
        }

        private async Task GenerateCert()
        {
            // Generate private-public key pair
            var rsaKey = RSA.Create(2048);

            // Describe certificate
            string subject = "CN=HardHat Manager";

            // Create certificate request
            var certificateRequest = new CertificateRequest(
                subject,
                rsaKey,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            );

            certificateRequest.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(
                    certificateAuthority: false,
                    hasPathLengthConstraint: false,
                    pathLengthConstraint: 0,
                    critical: true
                )
            );

            certificateRequest.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    keyUsages:
                        X509KeyUsageFlags.DigitalSignature
                        | X509KeyUsageFlags.KeyEncipherment,
                    critical: false
                )
            );

            certificateRequest.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(
                    key: certificateRequest.PublicKey,
                    critical: false
                )
            );

            var expireAt = DateTimeOffset.Now.AddYears(5);

            cert = certificateRequest.CreateSelfSigned(DateTimeOffset.Now, expireAt);

            // Export certificate with private key
            var exportableCertificate = new X509Certificate2(cert.Export(X509ContentType.Cert), (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet).CopyWithPrivateKey(rsaKey);


            //check if the operating system is windows or linux and only set friendlyName on windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                exportableCertificate.FriendlyName = "Certificate For Client Authorization";
            }

            // Create password for certificate protection
            var passwordForCertificateProtection = new SecureString();
            foreach (var @char in CertificatePassword)
            {
                passwordForCertificateProtection.AppendChar(@char);
            }

            // Export certificate to a file.
            var DirectorySeperatorChar = Path.DirectorySeparatorChar;
            string CertificateDir = $"{Environment.CurrentDirectory}{DirectorySeperatorChar}Certificates{DirectorySeperatorChar}";
            
            File.WriteAllBytes($"{CertificateDir}certClient{Name}.pfx",exportableCertificate.Export(X509ContentType.Pfx,passwordForCertificateProtection));
            CertificatePath = CertificateDir + $"certClient{Name}.pfx";
        }

    }
}
