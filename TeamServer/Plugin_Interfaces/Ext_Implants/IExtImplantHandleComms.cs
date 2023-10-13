using ApiModels.Plugin_Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TeamServer.Plugin_Interfaces.Ext_Implants
{
    //should deal with communication with implants externally both incoming and outgoing 
    public interface IExtimplantHandleComms
    {
       // public Task<IActionResult> HandleIExtImplantCommsAsync(IExtImplantMetadata engineermetadata, HttpContext httpContext,string pluginName);
    }
    public interface IExtImplantHandleCommsData : IPluginMetadata
    {
    }
}
