using FastEndpoints;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using HardHatCore.TeamServer.Plugin_Management;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class GetTasksForImplant : EndpointWithoutRequest<IEnumerable<ExtImplantTask_Base>>
    {
        public override void Configure()
        {
            Get("/Implants/{implantId}/tasks/");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            string implantId = Route<string>("implantId");
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var implant = _ImplantSvc.GetExtImplant(implantId);
            if (implant is null)
            {
                Console.WriteLine("Implant not found when trying to get tasks");
                SendNotFoundAsync();
            }
            var result = await implant.GetTasks();
            if (result is null)
            {
                AddError("implant was found, but failed to find any tasks");
                Console.WriteLine("Tasks not found");
            }
            if(result.Count() == 0)
            {
                AddError("no tasks found for implant", severity: FluentValidation.Severity.Info);
                SendNotFoundAsync();
            }
            ThrowIfAnyErrors();
            SendAsync(result);
        }
    }
}
