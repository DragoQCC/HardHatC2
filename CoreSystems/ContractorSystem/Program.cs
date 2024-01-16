using FastEndpoints;
using HardHatCore.ContractorSystem.Services;
using FastEndpoints.Swagger;

namespace HardHatCore.ContractorSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR();
            builder.Services.AddFastEndpoints().SwaggerDocument();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.MapHub<ContractorSystemHub>("/hardhatContractorHub");
            app.UseFastEndpoints().UseSwaggerGen();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            Init();
            app.Run();
        }

        public static async Task Init()
        {
            await Contractor_Database.InitDB();
            await Contractor_Database.ConnectDb();
        }
    }
}
