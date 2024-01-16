using FastEndpoints;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Management;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class GetImplantById : EndpointWithoutRequest<ExtImplant_Base>
    {
        public override void Configure()
        {
            Get("/Implants/{implantId}");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            string implantId = Route<string>("implantId");
            var Implant = _ImplantSvc.GetExtImplant(implantId);
            await SendAsync(Implant);
        }

    }
}
