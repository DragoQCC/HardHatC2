using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants
{
    //should deal with communication with implants externally both incoming and outgoing 
    public interface IExtimplantHandleComms : IExtImplantHandleCommsData
    {
        // key is the implantId, Value is a list of parent ids and ends with its own id, making its path. The path is a list where element 0 is the http implant, and each new element is a P2P implant a level deeper
        public static ConcurrentDictionary<string, List<string>> P2P_PathStorage = new();
        //the key is a parent id, the value is a list of child ids
        public static ConcurrentDictionary<string, List<string>> ParentToChildTracker = new();

        //contains pairs of socks proxies and thier Ids
        public static ConcurrentDictionary<string, Socks4Proxy> Proxy { get; set; } = new();

        //tracks which socks clients are connected to which proxy
        public static ConcurrentDictionary<string, string> SocksClientToProxyCache { get; set; } = new();

        string GetImplantType(IHeaderDictionary headers);
        T ExtractMetadata<T>(IHeaderDictionary headers, string pluginName) where T : IExtImplantMetadata;
        Task<ExtImplant_Base> GetCheckingInImplant(IExtImplantMetadata extImplantmetadata, HttpContext httpContext, IExtImplantService extImplantService_Base, string pluginName);
        Task HandleImplantRequest(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base, HttpContext httpContext);
        Task<byte[]> RespondToImplant(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base);
        Task<byte[]> ReturnImplantTasking(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base);
        bool SetHttpResponseHeaders(ref HttpContext httpContext, HttpManager httpManager);
        void CreateNewP2PPath(string implantId);
        Task HandleGetRequest(ExtImplant_Base implant);
        Task HandlePostRequest(ExtImplant_Base implant, HttpContext copiedHttpContext);
        Task SendTaskResultsToClient(ExtImplant_Base implant, IEnumerable<ExtImplantTaskResult_Base> results);
        Task<byte[]> HandlePreProcAndPackageTasking(ExtImplant_Base implant);
        void CreateNewEncKeyAndTaskUpdate(ExtImplant_Base implant, IExtImplantService extImplantService_Base);

        Task HandleC2Messages(List<C2Message> c2Messages);
        Task<IEnumerable<ExtImplantTaskResult_Base>> HandleTaskResults(IEnumerable<ExtImplantTaskResult_Base> taskResults, ExtImplant_Base implant);
        Task HandleCustomMessageType(C2Message message, ExtImplant_Base asset);

        Task HandleAssetNotification(IEnumerable<AssetNotification> notifs, ExtImplant_Base asset);

    }
    public interface IExtImplantHandleCommsData : IPluginMetadata
    {
    }
}
