using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants
{
    //should deal with communication with implants externally both incoming and outgoing 
    public interface IExtimplantHandleComms
    {
        // key is the implantId, Value is a list of parent ids and ends with its own id, making its path. The path is a list where element 0 is the http implant, and each new element is a P2P implant a level deeper
        public static Dictionary<string, List<string>> P2P_PathStorage = new Dictionary<string, List<string>>();
        //the key is a parent id, the value is a list of child ids
        public static Dictionary<string, List<string>> ParentToChildTracker = new Dictionary<string, List<string>>();

        Task<ExtImplant_Base> GetCheckingInImplant(IExtImplantMetadata extImplantmetadata, HttpContext httpContext, IExtImplantService extImplantService_Base, string pluginName);
        Task HandleImplantRequest(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base, HttpContext httpContext);
        Task<IActionResult> RespondToImplant(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base);
        Task<byte[]> ReturnImplantTasking(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base);
        bool SetHttpResponseHeaders(ref HttpContext httpContext, Httpmanager httpmanager);
        void CreateNewP2PPath(string implantId);
        Task HandleGetRequest(ExtImplant_Base implant);
        Task HandlePostRequest(ExtImplant_Base implant, IExtImplantService extImpService_base, HttpContext copiedHttpContext);
        Task<IEnumerable<ExtImplantTaskResult_Base>> ProcessTaskResults(IEnumerable<ExtImplantTaskResult_Base> taskResults, ExtImplant_Base implant);
        Task SendTaskResults(ExtImplant_Base implant, IEnumerable<ExtImplantTaskResult_Base> results);
        Task<byte[]> HandlePreProcAndPackageTasking(ExtImplant_Base implant);
        void CreateNewEncKeyAndTaskUpdate(ExtImplant_Base implant, IExtImplantService extImplantService_Base);
    }
    public interface IExtImplantHandleCommsData : IPluginMetadata
    {
    }
}
