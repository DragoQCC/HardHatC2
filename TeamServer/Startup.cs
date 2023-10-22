using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models.Database;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Services.Extra;
using HardHatCore.TeamServer.Utilities;

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

            services.AddSignalR();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TeamServer", Version = "v1" });
                c.CustomSchemaIds(type => type.ToString());

                // Configure Swagger to use OAuth2
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });
            services.AddSingleton<ImanagerService, managerService>();
            //services.AddSingleton<IEngineerService, EngineerService>();
            services.AddSingleton<IExtImplantService, ExtImplantService_Base>();

            string sqliteConnectionString = DatabaseService.ConnectionString;


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
                PluginService.InitPlugins();
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamServer v1");
                        // Enable OAuth2 in Swagger UI
                        c.OAuthUsePkce();
                    });
                    Console.WriteLine("TeamServer is running in development mode.");
                }

                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseIPWhitelist();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapHub<HardHatHub>("/HardHatHub");
                });
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
                if (String.IsNullOrEmpty(Encryption.UniversialMetadataKey))
                {
                    Console.WriteLine("Generating unique encryption keys for pathing and metadata id");
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
                Console.WriteLine("An error occured during startup");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
