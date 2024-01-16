using System.Collections.Generic;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;

namespace HardHatCore.TeamServer.Plugin_Management
{
    public class PluginHub
    {
        public IEnumerable<IExtImplantService> implant_servicePlugins { get; set; } = new List<IExtImplantService>();
        public IEnumerable<IExtimplantHandleComms> implant_commsPlugins { get; set; } = new List<IExtimplantHandleComms>();
        public IEnumerable<IExtImplant_TaskPreProcess> implant_preProcPlugins { get; set; } = new List<IExtImplant_TaskPreProcess>();
        public IEnumerable<IExtImplant_TaskPostProcess> implant_postProcPlugins { get; set; } = new List<IExtImplant_TaskPostProcess>();
        public IEnumerable<IExtImplant> implant_ModelPlugins { get; set; } = new List<ExtImplant_Base>();
        public IEnumerable<IAssetNotificationService> Asset_NotificationServicePlugins { get; set; } = new List<IAssetNotificationService>();
    }
}
