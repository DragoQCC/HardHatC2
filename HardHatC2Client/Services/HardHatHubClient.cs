using ApiModels.Responses;
using HardHatC2Client.Models;
using HardHatC2Client.Pages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace HardHatC2Client.Services
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

                //hub connection on run NewEngineer with no arguments
                _hubConnection.On("NewEngineer", async () =>
                {
                    await Engineers.GetAllEngineers();
                });
                
                //hub connection on the new engineer task response call Interact.UpdateTaskResponse pass in the engineerid string and the list of taskIds
                _hubConnection.On<string, List<string>>("ShowEngineerTaskResponse", async (engineerid, tasksIds) =>
                {
                    await Interact.UpdateTaskResponse(engineerid, tasksIds);
                });

                //hub connection on updateOutGoingTaskDic with 4 strings as arguments 
                _hubConnection.On<string, string, string, string>("UpdateOutgoingTaskDic", async (engineerid, taskid, command, arguments) =>
                {
                    await Interact.UpdateOutGoingTaskDic(engineerid, taskid, command, arguments);
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

                //hub connection on GetExistingTaskInfo taking a Dictionary<string, List<EngineerTask>> as the argument
                _hubConnection.On<Dictionary<string, List<EngineerTask>>>("GetExistingTaskInfo", async (taskInfoDic) =>
                {
                    await Interact.GetExistingTaskInfo(taskInfoDic);
                } );
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
                    await Engineers.SetPsCommand(pscmd);
                });

                //should take in a PivotProxy object and call a function on the PivotProxy page to add it to the list
                _hubConnection.On<PivotProxyItem>("AddPivotProxy", async (pivotProxy) =>
                {
                    await PivotProxy.AddPivotProxy(pivotProxy);
                });

                await _hubConnection.StartAsync();
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
        }        
    }
}
