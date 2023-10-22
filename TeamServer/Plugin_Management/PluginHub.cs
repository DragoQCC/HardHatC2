using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;

namespace HardHatCore.TeamServer.Plugin_Management
{
    [Export(typeof(IPluginHub))]
    public class PluginHub : IPluginHub
    {
        // imports are filled in by the container, you do not have to supply the type if you dont it will just use the type of the property
        //Any export declared with a matching contract will fulfill this import
        //An ordinary ImportAttribute attribute is filled by one and only one ExportAttribute. If more than one is available, the composition engine produces an error
        [ImportMany]
        public IEnumerable<Lazy<IExtImplantService, IExtImplantServiceData>> implant_servicePlugins { get; set; }
        [ImportMany]
        public IEnumerable<Lazy<IExtimplantHandleComms, IExtImplantHandleCommsData>> implant_commsPlugins { get; set; }
        [ImportMany]
        public IEnumerable<Lazy<IExtImplant_TaskPreProcess, IExtImplant_TaskPreProcessData>> implant_preProcPlugins { get; set; }
        [ImportMany]
        public IEnumerable<Lazy<IExtImplant_TaskPostProcess, IExtImplant_TaskPostProcessData>> implant_postProcPlugins { get; set; }
        [ImportMany]
        public IEnumerable<Lazy<ExtImplant_Base, IExtImplantData>> implant_ModelPlugins { get; set; }
    }
}
