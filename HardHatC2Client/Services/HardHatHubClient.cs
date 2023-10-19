using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Responses;
using HardHatCore.ApiModels.Shared;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using HardHatCore.HardHatC2Client.Components;
using HardHatCore.HardHatC2Client.Pages;
using HardHatCore.HardHatC2Client.Plugin_Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security;
using HardHatCore.HardHatC2Client.Models;
using HardHatCore.HardHatC2Client.Plugin_BaseClasses;
using HardHatCore.HardHatC2Client.Utilities;

namespace HardHatCore.HardHatC2Client.Services
{
    public class HardHatHubClient
    {
        public static string TeamserverIp { get; set; } = "https://localhost:5000";
        protected internal static Hub _hub;

        public static async Task CreateHubClient()
        {
            //if hub object does not exitst create it
            if (_hub == null)
            {
                _hub = new Hub();
                Console.WriteLine("Hub created");    
                await _hub.Connect();
            }
        }

        public class Hub
        {
            public HubConnection _hubConnection { get; private set; } // this way only the hub vlass can set the connection details but our other classes can still see what they are set to and the status. 

            public async Task Connect()
            {
                string teamip = TeamserverIp;
                //if the teamserIp still has http:// remove it from the string
                if (TeamserverIp.Contains("https://"))
                {
                    teamip = TeamserverIp.Replace("https://", "");
                }

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl($"https://{teamip}/HardHatHub", (opts) =>
                    {
                        opts.HttpMessageHandlerFactory = (message) =>
                        {
                            if (message is HttpClientHandler clientHandler)
                                // always verify the SSL certificate
                                clientHandler.ServerCertificateCustomValidationCallback +=
                                        (sender, certificate, chain, sslPolicyErrors) => { return true; };
                            return message;
                        };
                    })
                    .WithAutomaticReconnect()
                    .Build();

                //hubConnection.On is used to let the teamserver invoke things on the client

                //hub connection on run CheckInImplant with no arguments
                _hubConnection.On<ExtImplant_Base>("CheckInImplant", async (implant) =>
                {
                    await Implants.CheckInImplant(implant);
                });

                //hub connection on updateOutGoingTaskDic with 4 strings as arguments 
                _hubConnection.On<ExtImplant_Base, List<ExtImplantTask_Base>>("UpdateOutgoingTaskDic", async (implant, task) =>
                {
                    await ImplantInteract.UpdateOutGoingTaskDic(implant, task);
                });

                //hub connection on UpdateManagerList taking a Maanger object as the argument 
                _hubConnection.On<Manager>("UpdateManagerList", async (manager) =>
                {
                    await Managers.UpdateManagerList(manager);
                });
                //hub connection on GetExistingManagerList taking a Maanger object as the argument 
                _hubConnection.On<List<Manager>>("GetExistingManagerList", async (managers) =>
                {
                    await Managers.GetExistingManagerList(managers);
                });

                _hubConnection.On<DownloadFile>("AlertDownloadFile", (downloadFile) =>
                {
                    Downloads.DownloadedFiles.Add(downloadFile);
                });

                _hubConnection.On<HistoryEvent>("AlertEventHistory", async (historyEvent) =>
                {
                    await EventHistory.AddEvent(historyEvent);
                });

                _hubConnection.On<List<Cred>>("AddCreds", async (creds) =>
                {
                    await Credentials.AddCreds(creds);
                });

                _hubConnection.On<string>("AddPsCommand", async (pscmd) =>
                {
                    await Implants.SetPsCommand(pscmd);
                });

                //should take in a PivotProxy object and call a function on the PivotProxy page to add it to the list
                _hubConnection.On<PivotProxyItem>("AddPivotProxy", async (pivotProxy) =>
                {
                    await PivotProxy.AddPivotProxy(pivotProxy);
                });

                //hun connection on PushReconCenterEntity takes an ReconCenterEntity object as the argument and calls the AddReconCenterEntity function on the ReconCenter page
                _hubConnection.On<ReconCenterEntity>("PushReconCenterEntity", async (reconCenterEntity) =>
                {
                    Console.WriteLine("push reconCenter entity Invoked");
                    await ReconCenter.AddReconCenterEntity(reconCenterEntity);
                });

                //hub connection on PushReconCenterProperty taking a string as the EntityName and a ReconCenterEntityProperty as the argument and calls the AddReconCenterProperty function on the ReconCenter page
                _hubConnection.On<string, ReconCenterEntity.ReconCenterEntityProperty>("PushReconCenterProperty", async (entityName, reconCenterEntityProperty) =>
                {
                    Console.WriteLine("push reconCenter property Invoked");
                    await ReconCenter.AddReconCenterProperty(entityName, reconCenterEntityProperty);
                });

                _hubConnection.On<ReconCenterEntity.ReconCenterEntityProperty, ReconCenterEntity.ReconCenterEntityProperty>("PushReconCenterPropertyUpdate", async (oldProperty, newProperty) =>
                {
                    Console.WriteLine("push reconCenter property update Invoked");
                    await ReconCenter.UpdateReconCenterProperty(oldProperty, newProperty);
                });

                _hubConnection.On<string>("AddTaskToPickedUpList", async (taskid) =>
                {
                    await ImplantInteract.AddTaskToPickedUpList(taskid);
                });

                _hubConnection.On<List<HistoryEvent>>("GetExistingHistoryEvents", async (historyEvents) =>
                {
                    EventHistory.TimelineEvents.AddRange(historyEvents);
                });

                _hubConnection.On<List<DownloadFile>>("GetExistingDownloadedFiles", async (downloadedFiles) =>
                {
                    Downloads.DownloadedFiles.AddRange(downloadedFiles);
                });


                _hubConnection.On<List<PivotProxyItem>>("GetExistingPivotProxies", async (pivotProxies) =>
                {
                    PivotProxy._pivotProxyItems.AddRange(pivotProxies);
                });

                _hubConnection.On<List<Cred>>("GetExistingCreds", async (creds) =>
                {
                    await Credentials.AddCreds(creds);
                });

                _hubConnection.On<InteractiveTerminalCommand>("UpdateTabContent", async (interactiveTerminalCommand) =>
                {
                    await InteractiveTerminal.UpdateTabContent(interactiveTerminalCommand);
                });
                
                _hubConnection.On<IOCFile>("AddIOCFile", async (iocFile) =>
                {
                    await FileIOCTable.AddIOCFile(iocFile);
                });
                
                _hubConnection.On<string,CommandItem.OpsecStatus,string>("UpdateCommandOpsecLevelAndMitre", async (command, opsecLevel,MitreTechnique) =>
                {
                    ToolBoxes.updateOpsecStatusAndMitre(command, opsecLevel,MitreTechnique);
                });
                _hubConnection.On<CompiledImplant>("AddCompiledImplant", async (compImp) =>
                {
                    CompiledImplantTable.AddCompiledImplant(compImp);
                });
                _hubConnection.On<string,string>("UpdateImplantNote", async (engId,note) =>
                {
                    await Implants.UpdateImplantNote(engId,note);
                });
                _hubConnection.On<List<AliasEdit_Dialog.Alias>>("SendExistingAliases", async (aliases) =>
                {
                    AliasEdit_Dialog.inputAlises = aliases;
                });

                _hubConnection.On<ExtImplant_Base,List<string>>("SendTaskResults",async (implant, taskIds) =>
                {
                    await ImplantInteract.UpdateTaskResponse(implant,taskIds);
                });
                _hubConnection.On<string, string>("NotifyTaskDeletion", async (implantId, taskid) =>
                {
                    await ImplantInteract.NotifyTaskDeletion(implantId, taskid);
                });
                _hubConnection.On<string>("SharedNoteUpdated", async (content) =>
                {
                    await Notes_MD.UpdateSharedNoteContent(content);
                });
                _hubConnection.On<VncInteractionResponse, VNCSessionMetadata>("NotifyVNCInteractionResponse", async (vncResponse, VncMetadata) =>
                {
                    try
                    {
                        //Console.WriteLine("VNC Interaction Response Received");
                        Task.Run(async () =>
                        {
                            await VNC_Util.InitOrUpdateSession(VncMetadata, vncResponse);
                            await VNC_Util.UpdateDisplaySession(VncMetadata.SessionID);
                            while (!VNC_Util.VNCUIComponents.ContainsKey(VncMetadata.SessionID))
                            {
                                await Task.Delay(100);
                            }
                            await VNC_Util.VNCUIComponents[VncMetadata.SessionID].HandleUpdate(vncResponse, VncMetadata);
                        });

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    
                });

                await _hubConnection.StartAsync();
            }

            
            //these normal methods are used to invoke things on the teamserver from the client
            public async Task AddCred(Cred cred)
            {
                await _hubConnection.InvokeAsync("AddCred", arg1: cred );
            }
            
            public async Task<string> TriggerDownload(string StoredPath)
            {
                var result = await _hubConnection.InvokeAsync<string>("TriggerDownload", arg1: StoredPath);
                return result;
            }
           
            public async Task<string> HostFile(string file,string filename)
            {
                var result = await _hubConnection.InvokeAsync<string>("HostFile", arg1: file, arg2: filename);
                return result;
            }

            public async Task<string> CreateReconCenterEntity(ReconCenterEntity reconCenterEntity)
            {
                Console.WriteLine("Create ReconCenter Entity Invoked");
                var result = await _hubConnection.InvokeAsync<string>("CreateReconCenterEntity", arg1: reconCenterEntity);
                return result;
            }

            public async Task<string> CreateReconCenterProperty(string EntityName,ReconCenterEntity.ReconCenterEntityProperty reconCenterEntityProperty)
            {
                Console.WriteLine("Create ReconCenter Property Invoked");
                var result = await _hubConnection.InvokeAsync<string>("CreateReconCenterProperty", arg1: EntityName, arg2: reconCenterEntityProperty);
                return result;
            }

            public async Task<string> UpdateReconCenterProperty(ReconCenterEntity.ReconCenterEntityProperty oldProperty, ReconCenterEntity.ReconCenterEntityProperty newProperty)
            {
                Console.WriteLine("Update ReconCenter Property Invoked");
                var result = await _hubConnection.InvokeAsync<string>("UpdateReconCenterProperty", arg1: oldProperty, arg2: newProperty);
                return result;
            }

            public async Task<string> CancelRunningTask(string taskid,string engid)
            {
                Console.WriteLine("c2 hub cancel task called, invoking on ts");
                string CancelTaskId =  await _hubConnection.InvokeAsync<string>("CancelRunningTask", arg1: taskid, arg2: engid);
                return CancelTaskId;
            }

            public async Task<bool> CreateUser(string username, string password,string Role)
            {
                try
                {
                    Console.WriteLine("c2 hub create user called, invoking on ts");
                    string passwordHash = Hash.HashPassword(password, out byte[] salt);
                    bool result = await _hubConnection.InvokeAsync<bool>("CreateUser", arg1: username, arg2: passwordHash, arg3: salt,arg4:Role);
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return false;
                }

            }

            public async Task<bool> VerifyTokenUsernameExists(string username)
            {
                try
                {
                    Console.WriteLine("c2 hub login user called, invoking on ts");
                    var userExists = await _hubConnection.InvokeAsync<bool>("VerifyTokenUsernameExists", arg1: username);
                    return userExists;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return false;
                }
            }

            public async Task<byte[]> GetUserPasswordSalt(string username)
            {
                 return await _hubConnection.InvokeAsync<byte[]>("GetUserPasswordSalt", arg1: username);
            }

            public async Task PrettyLogs()
            {
                await _hubConnection.InvokeAsync("PrettyLogs");
            }

            public async Task CreateTerminalObject(InteractiveTerminalCommand terminalCommand)
            {
                await _hubConnection.InvokeAsync<InteractiveTerminalCommand>("CreateTerminalObject", arg1: terminalCommand);
                return;
            }

            public async Task CreateTabViewObject(InteractiveTerminal.TabView tabView)
            {
                await _hubConnection.InvokeAsync<InteractiveTerminal.TabView>("CreateTabViewObject", arg1: tabView);
                return;
            }

            public async Task<string> GetTerminalOutput(string commandId)
            {
                var result = await _hubConnection.InvokeAsync<string>("GetTerminalOutput", arg1: commandId);
                return result;
            }
            
            public async Task FireMitreAndOpsecUpdate(string command, CommandItem.OpsecStatus opsecLevel,string MitreTechnique)
            {
                await _hubConnection.InvokeAsync("UpdateCommandOpsecLevelAndMitre", arg1: command, arg2: opsecLevel,arg3:MitreTechnique);
            }
            public async Task<bool> CheckJWTExpiration(string token)
            {
                bool result = await _hubConnection.InvokeAsync<bool>("CheckJWTExpiration", arg1: token);
                return result;
            }

            public async Task AddNoteToImplant(string implantId, string note)
            {
                await _hubConnection.InvokeAsync("AddNoteToImplant", arg1: implantId, arg2: note);
            }

            public async Task<HardHatUser> RegisterHardHatUserAfterSignin(string username)
            {
                return await _hubConnection.InvokeAsync<HardHatUser>("RegisterHardHatUserAfterSignin", arg1: username);
            }

            public async Task UpdateTaskResponseSeenNotif(string username, string taskid,string engineerId)
            {
                await _hubConnection.InvokeAsync("UpdateTaskResponseSeenNotif", arg1: username, arg2: taskid, arg3:engineerId);
            }

            public async Task CreateorUpdateAlias(string username, AliasEdit_Dialog.Alias alias)
            {
                await _hubConnection.InvokeAsync("CreateorUpdateAlias", arg1: username, arg2: alias);
            }

            public async Task<List<AliasEdit_Dialog.Alias>> GetExistingAliases(string username)
            {
                return await _hubConnection.InvokeAsync<List<AliasEdit_Dialog.Alias>>("GetExistingAliases", arg1: username);
            }

            public async Task<bool> AddWebhook(Webhooks.Webhook webhook)
            {
                return await _hubConnection.InvokeAsync<bool>("CreateOrUpdateWebhook", arg1:webhook);
            }

            public async Task<List<Webhooks.Webhook>> GetExistingWebhooks()
            {
                return await _hubConnection.InvokeAsync<List<Webhooks.Webhook>>("GetExistingWebhooks");
            }

            //public async Task<bool> DeleteWebhook(Webhooks.Webhook webhook)
            //{
            //    return await _hubConnection.InvokeAsync<bool>("DeleteWebhook", arg1:webhook);
            //}

            public async Task<bool> RefreshTeamserverPlugins()
            {
                return await _hubConnection.InvokeAsync<bool>("RefreshTeamserverPlugins");
            }

            public async Task NotifySharedNoteUpdate(string noteContnet)
            { 
                await _hubConnection.InvokeAsync("NotifySharedNoteUpdate", arg1: noteContnet);
            }
            public async Task VNCsendHeartbeatRequestToServer(string implantId, string vncSessionId)
            {
                await _hubConnection.InvokeAsync("VNCsendHeartbeatToServer", arg1: implantId, arg2: vncSessionId, arg3: "");
            }
           
            public async Task VNCsendMouseClickToServer(double x, double y, long button,string implantId, string vncSessionId)
            {
                await _hubConnection.InvokeAsync("VNCsendMouseClickToServer", arg1: x, arg2: y, arg3: button, arg4: implantId, arg5:vncSessionId, arg6: Login.SignedInUser);
            }

            public async Task VNCsendMouseMoveToServer(double x, double y, string implantId, string vncSessionId)
            { 
                await _hubConnection.InvokeAsync("VNCsendMouseMoveToServer", arg1: x, arg2: y, arg3: implantId, arg4: vncSessionId, arg5: Login.SignedInUser);
            }

            public async Task VNCSendTextToServer(string text, string implantId, string vncSessionId)
            {
                await _hubConnection.InvokeAsync("VNCSendTextToServer", arg1: text, arg2: implantId, arg3: vncSessionId, arg4: Login.SignedInUser);
            }

            public async Task VNCsendGetClipboardToServer(string implantId, string vncSessionId)
            {
                await _hubConnection.InvokeAsync("VNCsendGetClipboardToServer", arg1: implantId, arg2: vncSessionId);
            }
            public async Task VNCsendClipboardDataToServer(string clipboardData, string implantId, string vncSessionId)
            {
                await _hubConnection.InvokeAsync("VNCsendClipboardDataToServer", arg1: clipboardData, arg2: implantId, arg3: vncSessionId, arg4: Login.SignedInUser);
            }

            public async Task<List<string>> GetBindableAddressesOnServer()
            {
                return await _hubConnection.InvokeAsync<List<string>>("GetBindableAddressesOnServer");
            }
        }        
    }
}
