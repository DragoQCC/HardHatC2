using FastEndpoints;
using HardHatCore.ContractorSystem.Services;
using FastEndpoints.Swagger;
using HardHatCore.ContractorSystem.Services.Communication;
using System.Net.WebSockets;
using Microsoft.AspNetCore.WebSockets;
using ProtoBuf.Grpc.Server;

namespace HardHatCore.ContractorSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSignalR();
            builder.Services.AddCodeFirstGrpc();
            builder.Services.AddWebSockets(options => 
            {
                options.KeepAliveInterval = TimeSpan.FromSeconds(300);
            });
            builder.Services.AddSingleton<ContractSystemFactory>();

            //builder.WebHost.ConfigureKestrel(options => 
            //{
            //    options.ListenAnyIP(5002, listenOptions =>
            //    {
            //        listenOptions.UseHttps();
            //        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
            //    });
            //});

            var app = builder.Build();
            

            //map SignalR hub, websocket handler, and gRPC service
            app.MapHub<HH_SignalRComms>("/hardhatContractorHub");
            app.UseWebSockets();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapGrpcService<HH_GrpcComms>();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/hardhatContractorHub")
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        // Handle as SignalR request
                        await next();
                    }
                    else
                    {
                        // Handle as plain WebSocket request
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await HH_WebSocketComms.HandleWebSocket(webSocket);
                    }
                }
                else
                {
                    await next();
                }
            });

            await Init();
            app.Run();
        }

        public static async Task Init()
        {
            await Contractor_Database.InitDB();
            await Contractor_Database.ConnectDb();
        }
    }
}
