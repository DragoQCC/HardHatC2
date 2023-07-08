using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using TeamServer.Services;
using TeamServer.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using TeamServer.Services.Extra;
using TeamServer.Models.Database;

namespace TeamServer
{
    public class Startup
    {
        public static string TeamserverIP { get; private set;}
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
            });
            services.AddSingleton<ImanagerService, managerService>();
            services.AddSingleton<IEngineerService, EngineerService>();

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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TeamServer v1"));
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
            TeamserverIP = app.ServerFeatures.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>().Addresses.First();
            Console.WriteLine("TeamServer is running on " + TeamserverIP);
            LoggingService.Init();
            Console.WriteLine("Initiating SQLite server");
            DatabaseService.Init();
            RestClientOptions options = new RestClientOptions($"{TeamserverIP}");
            //options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            //client = new RestClient(options);
            Console.WriteLine("Connecting to database");
            DatabaseService.ConnectDb();
            Console.WriteLine("Creating tables");
            DatabaseService.CreateTables().Wait();
            Console.WriteLine("Creating default roles");
            UsersRolesDatabaseService.CreateDefaultRoles().Wait();
            Console.WriteLine("Creating default admin");
            UsersRolesDatabaseService.CreateDefaultAdmin().Wait();
            Console.WriteLine("Filling teamserver from database");
            DatabaseService.FillTeamserverFromDatabase().ContinueWith((task) =>
            {
                if (String.IsNullOrEmpty(Encryption.UniversialMetadataKey))
                {
                    Console.WriteLine("Generating unique encryption keys for pathing and metadata id");
                    Encryption.GenerateUniversialKeys();
                }
            }).Wait();
           
        }
    }
}
