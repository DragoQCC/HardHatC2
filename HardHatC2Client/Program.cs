using System.Security.Cryptography.X509Certificates;
using Blazored.LocalStorage;
using Blazored.Toast;
using ElectronNET.API;
using ElectronNET.API.Entities;
using HardHatCore.HardHatC2Client.Plugin_Management;
using HardHatCore.HardHatC2Client.Services;
using HardHatCore.HardHatC2Client.Utilities;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using MudBlazor.Extensions;
using MudBlazor.Services;
using RestSharp;
using Toolbelt.Blazor.Extensions.DependencyInjection;

internal class Program
{
    public static bool isElectron = false;

    private static async Task Main(string[] args)
    {
        try
        {
            //read the appsettings.json file for the AutoInstallCert option
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();

            var builder = WebApplication.CreateBuilder(args);

            //check the LaunchMode value for Electron
            string launchMode = config.GetValue<string>("LaunchMode");
            string teamserverIP = config.GetValue<string>("teamserver_ip");
            if (launchMode.Equals("electron", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine("Setting up Electron Mode");
                builder.WebHost.UseElectron(args);
                builder.Services.AddElectron();
                isElectron = true;
            }
            // Add services to the container.
            PluginService.InitPluginsWithCustomContext();
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            // default is https://localhost:5000 , update in appsettings.json as needed 
            RestClientOptions options = new RestClientOptions(teamserverIP);

            builder.Services.AddBlazoredToast();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddHotKeys2();

            builder.Services.AddSignalR(c => 
            {
                c.MaximumReceiveMessageSize = long.MaxValue;
                c.StreamBufferCapacity = 100;
                c.MaximumParallelInvocationsPerClient = 100;
                c.EnableDetailedErrors = true;
            });
            
            builder.Services.AddMudServices();
            builder.Services.AddMudMarkdownServices();
            MudExtensions.Services.ExtensionServiceCollectionExtensions.AddMudExtensions(builder.Services);
            builder.Services.AddMudExtensions();


            
            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            builder.Services.AddSingleton(new RestClient(new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true }), options));

            // add jwt authentication
            builder.Services.AddAuthenticationCore();

            builder.Services.AddScoped<AuthenticationStateProvider, MyAuthenticationStateProviderService>();

            //only generate cert if it doesnt exist
            
            await CertGen.SetCertificatePath();
            if (!File.Exists(CertGen.CertificatePath))
            {
                Console.WriteLine("Generating Cert");
                await CertGen.GenerateCert();
            }
            else
            {
                Console.WriteLine("Loading Cert");
                await CertGen.LoadExistingCert();
            }
            builder.WebHost.ConfigureKestrel(serverOptions =>
                            {
                                serverOptions.ConfigureEndpointDefaults(listenOptions =>
                                {
                                    listenOptions.UseHttps(new X509Certificate2(CertGen.CertificatePath, CertGen.CertificatePassword));

                                });
                            });
            var app = builder.Build();



            
            //if AutoInstallCert is true then install the cert to the root store
            if (config.GetValue<bool>("AutoInstallCert"))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        using (var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                        {
                            Console.WriteLine("Adding cert to root store");
                            store.Open(OpenFlags.ReadWrite);
                            store.Add(CertGen.cert);
                            Console.WriteLine("Cert added to root store");
                            store.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                });
            }
            else
            {
                Console.WriteLine("AutoInstallCert is false, if you wish to automatically trust the cert edit the appsettings.json file");
            }
        

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
        
        

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.UseMudExtensions();
            if (isElectron)
            {
                Console.WriteLine("Launching in Electron Mode");
                await app.StartAsync();
                // Open the Electron-Window here
                var window = await Electron.WindowManager.CreateWindowAsync("https://localhost:8002/");
                window.OnClosed += () => {
                    Electron.App.Quit();
                };
                app.WaitForShutdown();
            }
            else
            {
                app.Run();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}

