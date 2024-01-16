using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_BaseClasses;

namespace HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants
{
    public interface IExtImplant_TaskPreProcess : IExtImplant_TaskPreProcessData
    {
        bool DetermineIfTaskPreProc(ExtImplantTask_Base task);
        void PreProcessTask(ExtImplantTask_Base task, ExtImplant_Base implant);
    }
    public interface IExtImplant_TaskPreProcessData : IPluginMetadata
    {
    }
}
