using System;
using System.Threading.Tasks;
using System.Threading;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using FastEndpoints;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class CreateImplant : Endpoint<ExtImplantCreateRequest_Base,string>
    {
        public override void Configure()
        {
            Post("Implants/");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(ExtImplantCreateRequest_Base implantSpwnRequest, CancellationToken ct)
        {
            try
            {
                bool isCreated = false;
                string result_message = null;
                var svc_plugins = Plugin_Management.PluginService.pluginHub.implant_servicePlugins;
                var svc_plugin = svc_plugins.GetPluginEnumerableResult(implantSpwnRequest.implantType);

                isCreated = svc_plugin.CreateExtImplant(implantSpwnRequest, out result_message);
                if (isCreated)
                {
                    SendOkAsync(result_message);
                }
                else
                {
                    Console.WriteLine("error in implant creation : " + result_message);
                    ThrowError(result_message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error in implant creation : " + ex.Message);
                ThrowError(ex.Message);
            }

        }
    }
}
