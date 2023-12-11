using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Shared;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Models.InteractiveTerminal;
using HardHatCore.TeamServer.Models.Managers;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using System.Collections.Generic;
using System.Threading.Tasks;
using static HardHatCore.TeamServer.Models.Extras.HelpMenuItem;
using static HardHatCore.TeamServer.Models.Extras.ReconCenterEntity;

namespace HardHatCore.TeamServer.Services
{
    public interface IHardHatHub
    {
        Task PushReconCenterEntity(ReconCenterEntity reconCenterEntity);
        Task PushReconCenterProperty(string entityName, ReconCenterEntityProperty reconCenterProperty);
        Task PushReconCenterPropertyUpdate(ReconCenterEntityProperty oldProperty, ReconCenterEntityProperty newProperty);
        Task SharedNoteUpdated(string content);
        Task AddCreds(List<Cred> creds);
        Task UpdateCommandOpsecLevelAndMitre(string command, OpsecStatus opsecLevel, string mitreTechnique);
        Task UpdateImplantNote(string engId, string note);
        Task CheckInImplant(ExtImplant_Base implant);
        Task UpdateOutgoingTaskDic(ExtImplant_Base implant, List<ExtImplantTask_Base> task);
        Task UpdateManagerList(manager manager);
        Task GetExistingManagerList(List<Httpmanager> httpManagers);
        Task GetExistingManagerList(List<SMBmanager> smbManagers);
        Task GetExistingManagerList(List<TCPManager> tcpManagers);
        Task GetExistingTaskInfo(ExtImplant_Base implant, List<ExtImplantTask_Base> results);
        Task GetExistingHistoryEvents(List<HistoryEvent> historyEvents);
        Task GetExistingDownloadedFiles(List<DownloadFile> downloadedFiles);
        Task GetExistingUploadedFiles(List<UploadedFile> uploadedFiles);
        Task GetExistingPivotProxies(List<PivotProxy> pivotProxies);
        Task GetExistingCreds(List<Cred> creds);
        Task AlertDownloadFile(DownloadFile downloadFile);
        Task AlertEventHistory(HistoryEvent historyEvent);
        Task AddPsCommand(string psCommand);
        Task AddPivotProxy(PivotProxy pivotProxy);
        Task AddTaskToPickedUpList(string taskId);
        Task UpdateTabContent(InteractiveTerminalCommand interactiveTerminalCommand);
        Task AddIOCFile(IOCFile iocFile);
        Task AddCompiledImplant(CompiledImplant compiledImplant);
        Task SendTaskResults(ExtImplant_Base implant, List<string> taskIds);
        Task NotifyTaskDeletion(string implantId, string taskId);
        Task NotifyVNCInteractionResponse(VncInteractionResponse vncResponse, VNCSessionMetadata vncSessionMetadata);
    }
}
