using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using FastEndpoints;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Management;
using Microsoft.AspNetCore.Http;
using HardHatCore.TeamServer.Plugin_BaseClasses;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class GetTaskResultsForImplant : EndpointWithoutRequest<IEnumerable<ExtImplantTaskResult_Base>>
    {
        public override void Configure()
        {
            Get("/Implants/{implantId}/taskresults/");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            string implantId = Route<string>("implantId");
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            ExtImplant_Base? implant = _ImplantSvc.GetExtImplant(implantId);
            if (implant is null)
            {
                Console.WriteLine("Implant not found when trying to get task results");
                SendNotFoundAsync();
            }
            var result = await implant.GetTaskResults();
            if (result is null)
            {
                Console.WriteLine("Tasks not found when trying to get task results");
                SendNotFoundAsync();
            }
            SendAsync(result);
        }
    }
}
