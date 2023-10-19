using System.Security.Cryptography.X509Certificates;
using Blazored.LocalStorage;
using Blazored.Toast;
using HardHatCore.HardHatC2Client.Services;
using HardHatCore.HardHatC2Client.Utilities;
using RestSharp;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using HardHatCore.HardHatC2Client.Plugin_Management;
using MudBlazor.Extensions;
using MudBlazor;
using System.Diagnostics;
using System.Security;
using ICSharpCode.Decompiler.IL;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        PluginService.InitPlugins();
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        // if args[0] is null then options is http://localhost:5000 else its args[0]
        RestClientOptions options = new RestClientOptions("https://localhost:5000");
        if (args.Count() > 0)
        {
            if (args[0] != null)
            {
                options = new RestClientOptions(args[0]);
                HardHatHubClient.TeamserverIp = args[0];
            }
        }
        builder.Services.AddBlazoredToast();
        builder.Services.AddBlazoredLocalStorage();
        builder.Services.AddSignalR();
        builder.Services.AddMudServices();
        builder.Services.AddMudMarkdownServices();
        MudExtensions.Services.ExtensionServiceCollectionExtensions.AddMudExtensions(builder.Services);
        MudBlazor.Extensions.ServiceCollectionExtensions.AddMudExtensions(builder.Services);
 


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

        //read the appsettings.json file for the AutoInstallCert option
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false).Build();
        
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
        app.Run();
    }
}

