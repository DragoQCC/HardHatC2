using System.Threading.Tasks;
using System.Threading;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Models.Managers;
using HardHatCore.TeamServer.Services;
using FastEndpoints;
using HardHatCore.ApiModels.Requests;
using Microsoft.AspNetCore.Http;
using System;

namespace HardHatCore.TeamServer.Endpoints.ManagersEndpoints
{
    public class StartManager : Endpoint<StartManagerRequest,Manager>
    {
        public override void Configure()
        {
            Post("/Managers");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Managers"));
        }

        public override async Task HandleAsync(StartManagerRequest request , CancellationToken ct)
        {
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }

            Manager manager = null;
            Console.WriteLine("processing create manager request");
            if (request.managertype == ManagerType.http || request.managertype == ManagerType.https)
            {
                manager = HttpManager.HttpManagerFactoryFunc(request.Name, request.ConnectionAddress, request.ConnectionPort, request.BindAddress, request.BindPort, request.IsSecure, request.C2profile);
                DatabaseService.AsyncConnection.InsertAsync((HttpManager_DAO)manager);
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"{manager.Type} Manager {manager.Name} created on {((HttpManager)manager).ConnectionAddress}:{((HttpManager)manager).ConnectionPort}", Status = "success" });
                LoggingService.EventLogger.Information($"{manager.Type} Manager created.{@manager}", manager);
            }
            else if (request.managertype == ManagerType.tcp)
            {
                if (request.connectionMode == ConnectionMode.bind)
                {
                    manager = new TCPManager(request.Name, request.ListenPort, request.IsLocalHost);
                }
                else if (request.connectionMode == ConnectionMode.reverse)
                {
                    manager = new TCPManager(request.Name, request.ConnectionAddress, request.BindPort);
                }
                DatabaseService.AsyncConnection.InsertAsync((TCPManager_DAO)manager);
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"{((TCPManager)manager).connectionMode} TCP Manager {manager.Name}, created", Status = "success" });
                LoggingService.EventLogger.Information("TCP Manager created.{@Manager}", manager);
            }
            else if (request.managertype == ManagerType.smb)
            {
                if (request.connectionMode == ConnectionMode.bind)
                {
                    manager = new SMBManager(request.Name, request.NamedPipe);
                }
                else if (request.connectionMode == ConnectionMode.reverse)
                {
                    manager = new SMBManager(request.Name, request.NamedPipe, request.ConnectionAddress);
                }
                DatabaseService.AsyncConnection.InsertAsync((SMBManager_DAO)manager);
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"SMB Manager {manager.Name}, with pipe name {((SMBManager)manager).NamedPipe}", Status = "success" });
                LoggingService.EventLogger.Information("SMB Manager created.{@Manager}", manager);
            }

            
            Console.WriteLine("Manager created");
            ImanagerService.Addmanager(manager);
            await HardHatHub.UpdateManagerList(manager);
            manager.Start();
            await SendOkAsync(manager);
        }
    }
}
