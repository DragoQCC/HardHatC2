using System.Collections.Generic;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_BaseClasses;

namespace HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants
{
    public interface IExtImplant_TaskPostProcess : IExtImplant_TaskPostProcessData
    {
        bool DetermineIfTaskPostProc(ExtImplantTask_Base task);
        Task PostProcessTask(IEnumerable<ExtImplantTaskResult_Base> results, ExtImplantTaskResult_Base result, ExtImplant_Base extImplant, ExtImplantTask_Base task);
    }
    public interface IExtImplant_TaskPostProcessData : IPluginMetadata
    {
    }
}
