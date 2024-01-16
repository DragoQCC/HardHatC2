using System;
using System.Threading.Tasks;
using System.Threading;
using FastEndpoints;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.TeamServer.Endpoints.DTOs;
using HardHatCore.TeamServer.Plugin_Management;
using Microsoft.AspNetCore.Http;
using HardHatCore.TeamServer.Plugin_BaseClasses;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class GetTaskResult : Endpoint<GetTaskRequest,ExtImplantTaskResult_Base>
    {
        public override void Configure()
        {
            Get("/Implants/{implantId}/taskresults/{taskId}");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(GetTaskRequest gettaskReq, CancellationToken ct)
        {
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            ExtImplant_Base? implant = _ImplantSvc.GetExtImplant(gettaskReq.implantId);
            if (implant is null)
            {
                Console.WriteLine("Implant not found when trying to get task results");
                ThrowError("Implant not found when trying to get task results");
            }
            var result = await implant.GetTaskResult(gettaskReq.taskId);
            if (result is null)
            {
                Console.WriteLine("Task not found when trying to get task results");
                ThrowError("Task not found when trying to get task results");
            }
            SendAsync(result);
        }
    }
}
