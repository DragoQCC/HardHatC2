using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Services;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.ManagersEndpoints
{
    public class GetManagers : EndpointWithoutRequest<IEnumerable<Manager>>
    {
        public override void Configure()
        {
            Get("/Managers/");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Managers"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var managers = ImanagerService.Getmanagers();
            SendOkAsync(managers);
        }
    }
}
