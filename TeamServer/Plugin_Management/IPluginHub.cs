using System.Collections.Generic;
using System;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;

namespace HardHatCore.TeamServer.Plugin_Management
{
    public interface IPluginHub
    {
        // all the plugin interface objects or IEnumerables I want to have for imports directly into the PluginHub  
        // does not have to contain every plugin for example I might just want to import a IExternalImplant type and that holds more imports for what it needs etc. 
       
        IEnumerable<Lazy<IExtImplantService, IExtImplantServiceData>> implant_servicePlugins { get; set; }
        IEnumerable<Lazy<IExtimplantHandleComms, IExtImplantHandleCommsData>> implant_commsPlugins { get; set; }
        IEnumerable<Lazy<IExtImplant_TaskPreProcess, IExtImplant_TaskPreProcessData>> implant_preProcPlugins { get; set; }
        IEnumerable<Lazy<IExtImplant_TaskPostProcess, IExtImplant_TaskPostProcessData>> implant_postProcPlugins { get; set; }
        IEnumerable<Lazy<ExtImplant_Base, IExtImplantData>> implant_ModelPlugins { get; set; }
    }
}
