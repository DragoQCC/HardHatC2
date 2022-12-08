//using HardHatC2Client.Data;
using AutoMapper;
using Blazored.Toast;
using HardHatC2Client.Mapping;
using HardHatC2Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RestSharp;
using MudBlazor.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
builder.Services.AddSingleton(new RestClient(new HttpClient( new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true }), options));

//var mapperConfiguration = new MapperConfiguration(configuration =>
//{
//    configuration.AddProfile(new MyProfile());
//});
//var mapper = mapperConfiguration.CreateMapper();
//builder.Services.AddSingleton(mapper);
builder.Services.AddBlazoredToast();
builder.Services.AddSignalR();
builder.Services.AddMudServices();

var app = builder.Build();

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

app.Run();
