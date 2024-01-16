using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Authorization;
using FastEndpoints;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class GetImplants : EndpointWithoutRequest<IEnumerable<ExtImplant_Base>>
    {
        public override void Configure()
        {
            Get("/Implants");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var Implants = _ImplantSvc.GetExtImplants();
            await SendAsync(Implants);
        }

    }
}
