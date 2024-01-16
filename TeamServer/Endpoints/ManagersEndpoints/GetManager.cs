using System.Threading.Tasks;
using System.Threading;
using FastEndpoints;
using HardHatCore.TeamServer.Models;
using Microsoft.AspNetCore.Http;
using HardHatCore.TeamServer.Services;

namespace HardHatCore.TeamServer.Endpoints.ManagersEndpoints
{
    public class GetManager : EndpointWithoutRequest<Manager>
    {
        public override void Configure()
        {
            Get("/Managers/{name}");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Managers"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            string name = Route<string>("name");
            Manager manager = ImanagerService.Getmanager(name);
            SendAsync(manager);
        }
    }
}
