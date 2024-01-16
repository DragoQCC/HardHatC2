using System;
using System.Linq;
using FastEndpoints;
using HardHatCore.TeamServer.Models.Database;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Services.Extra;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestSharp;
using FastEndpoints.Swagger;
using FastEndpoints.Security;

namespace HardHatCore.TeamServer
{
    public class Startup
    {
        public static string TeamserverIP { get; internal set;}
        public static RestClient client { get; private set; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSignalR(c => 
            {
                c.EnableDetailedErrors = true;
                c.MaximumParallelInvocationsPerClient = 100;
                c.StreamBufferCapacity = 100;
                c.MaximumReceiveMessageSize = int.MaxValue;
            });
            services.AddFastEndpoints(c=>
            {
                //take all endpoints except the HandleAsset endpoint
                c.Filter = (x =>
                {
                    if (x.Name.Contains("HandleAsset")) { return false; }
                    return true;
                });

            }).SwaggerDocument(c =>
            {
                c.DocumentSettings = s =>
                {
                    s.Title = "TeamServer";
                    s.Version = "v1";
                    s.Description = "TeamServer API";
                };
                c.EnableGetRequestsWithBody = false;
                c.EnableJWTBearerAuth = true;


            });
            //services.AddControllers();
            
            services.AddSingleton<ImanagerService, managerService>();
            services.AddSingleton<IExtImplantService, ExtImplantService_Base>();

            services.AddTransient<IUserStore<UserInfo>, UserStore>();
            services.AddTransient<IRoleStore<RoleInfo>, RoleStore>();
            services.AddTransient<UserManager<UserInfo>, MyUserManager>();
            
            services.AddIdentity<UserInfo, RoleInfo>().AddDefaultTokenProviders();
            services.Configure<IPWhitelistOptions>(Configuration.GetSection("IPWhitelistOptions"));

            //add role-based authorization services
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(j =>
            {
                j.SaveToken = true;
                j.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Issuer"]
                };
            });

            Authentication.SignInManager = services.BuildServiceProvider().GetService<SignInManager<UserInfo>>();
            Authentication.UserManager = services.BuildServiceProvider().GetService<UserManager<UserInfo>>();
            Authentication.Configuration = Configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            try
            {
                PluginService.InitPluginsWithCustomContext();
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    Console.WriteLine("TeamServer is running in development mode.");
                }
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseIPWhitelist();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapFastEndpoints();
                    //endpoints.MapControllers();
                    endpoints.MapHub<HardHatHub>("/HardHatHub");
                });
                app.UseSwaggerGen();
                Seralization.Init();
                //add a check to make sure the Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses is not empty
                if(String.IsNullOrEmpty(TeamserverIP))
                {
                    if (app.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses.Count > 0)
                    {
                        TeamserverIP = app.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses.First();
                    }
                    else
                    {
                        Console.WriteLine("Ip address to run teamserver not found defaulting to https://localhost:5000.");
                        TeamserverIP = "https://localhost:5000";
                    }
                }
                Console.WriteLine("TeamServer is running on " + TeamserverIP);
                LoggingService.Init();
                Console.WriteLine("Initiating SQLite server");
                DatabaseService.Init();
                Console.WriteLine("Connecting to database");
                DatabaseService.ConnectDb();
                Console.WriteLine("Creating tables");
                await DatabaseService.CreateTables();
                Console.WriteLine("Creating default roles");
                await UsersRolesDatabaseService.CreateDefaultRoles();
                Console.WriteLine("Creating default admin");
                await UsersRolesDatabaseService.CreateDefaultAdmin();
                Console.WriteLine("Filling teamserver from database");
                await DatabaseService.FillTeamserverFromDatabase();
                if (String.IsNullOrEmpty(Encryption.UniversialMessageKey))
                {
                    Console.WriteLine("Generating unique encryption keys for server");
                    Encryption.GenerateUniversialKeys();
                }

                //make a timer that every 30 seconds runs the ManageImplantStatusUpdate() function in the implant service
                await IExtImplantService.ManageImplantStatusUpdate();
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 30000;
                timer.Start();
                timer.AutoReset = true;
                timer.Elapsed += (sender, e) => { IExtImplantService.ManageImplantStatusUpdate(); };
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred during startup");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
