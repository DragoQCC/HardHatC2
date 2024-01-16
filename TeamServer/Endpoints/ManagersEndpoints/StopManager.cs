using System.Threading.Tasks;
using System.Threading;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Services;
using System.Xml.Linq;
using HardHatCore.TeamServer.Models.Extras;

namespace HardHatCore.TeamServer.Endpoints.ManagersEndpoints
{
    public class StopManager : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Delete("/Managers/{name}");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Managers"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            string name = Route<string>("name");
            var manager = ImanagerService.Getmanager(name);
            if (manager is null)
            {
                ThrowError("manager not found");
            }

            manager.Stop();
            ImanagerService.Removemanager(manager);
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.DeleteAsync((HttpManager_DAO)manager);
            HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Manager {manager.Name} removed", Status = "warning" });
            LoggingService.EventLogger.Warning("Manager {Manager.Name} removed", manager.Name);
            SendOkAsync();
        }
    }
}
