using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using TeamServer.Plugin_BaseClasses;
using TeamServer.Plugin_Interfaces.Ext_Implants;

namespace TeamServer.Plugin_Management
{
    [Export(typeof(IPluginHub))]
    public class PluginHub : IPluginHub
    {
        // imports are filled in by the container, you do not have to supply the type if you dont it will just use the type of the property
        //Any export declared with a matching contract will fulfill this import
        //An ordinary ImportAttribute attribute is filled by one and only one ExportAttribute. If more than one is available, the composition engine produces an error
        [ImportMany]
        public IEnumerable<Lazy<ExtImplantService_Base, IExtImplantServiceData>> implant_servicePlugins { get; set; }
        [ImportMany]
        public IEnumerable<Lazy<ExtImplantHandleComms_Base, IExtImplantHandleCommsData>> implant_commsPlugins { get; set; }
        [ImportMany]
        public IEnumerable<Lazy<ExtImplant_TaskPreProcess_Base, IExtImplant_TaskPreProcessData>> implant_preProcPlugins { get; set; }
        [ImportMany]
        public IEnumerable<Lazy<ExtImplant_TaskPostProcess_Base, IExtImplant_TaskPostProcessData>> implant_postProcPlugins { get; set; }
        [ImportMany]
        public IEnumerable<Lazy<ExtImplant_Base, IExtImplantData>> implant_ModelPlugins { get; set; }
    }
}
