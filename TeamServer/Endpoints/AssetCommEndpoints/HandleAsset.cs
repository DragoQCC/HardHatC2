using HardHatCore.TeamServer.Endpoints.DTOs;
using System.Threading.Tasks;
using System.Threading;
using System;
using FastEndpoints;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Management;
using Microsoft.AspNetCore.Http;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Endpoints.ConfigModels;

namespace HardHatCore.TeamServer.Endpoints.AssetCommEndpoints
{
    public class HandleAsset : EndpointWithoutRequest
    {
        private IExtimplantHandleComms assetComm_service { get; set; }
        public AssetCommUrls assetUrls { get; set; }
        

        public override void Configure()
        {
            Verbs(Http.GET,Http.POST);
            Routes(assetUrls.Urls);
        }

        

        public override async Task HandleAsync(CancellationToken ct)
        {
            try
            {
                if(assetComm_service is null)
                {
                    assetComm_service = PluginService.GetImpCommsPlugin("Default");
                }
                string implant_name = assetComm_service.GetImplantType(HttpContext.Request.Headers);
                // by this point we have gotten back data from the eng either for a check in or a task response.
                // DeEncrypt the HttpRequest using the Encryption.AES_Decrypt function
                var implantMetadata = assetComm_service.ExtractMetadata<ExtImplantMetadata_Base>(HttpContext.Request.Headers, implant_name);
                //var comm_base = PluginService.GetImpCommsPlugin(implant_name);
                var commBase = PluginService.GetImpCommsPlugin(implant_name);
                if (implantMetadata != null)
                {
                    IExtImplantService extImplantService_Base = Plugin_Management.PluginService.GetImpServicePlugin(implant_name);
                    ExtImplant_Base implant = await commBase.GetCheckingInImplant(implantMetadata, HttpContext, extImplantService_Base, implant_name);
                    //handle the implant request and respond to it
                    await commBase.HandleImplantRequest(implant, extImplantService_Base, HttpContext);
                    var taskingResponse = await commBase.RespondToImplant(implant, extImplantService_Base);
                    if (taskingResponse.Length > 0)
                    {
                        SendBytesAsync(taskingResponse);
                    }
                    else
                    {
                        SendNoContentAsync();
                    }
                }
                else
                {
                    SendErrorsAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                SendErrorsAsync();
            }
        }
    }
}
