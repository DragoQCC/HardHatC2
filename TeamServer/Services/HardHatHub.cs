using ApiModels.Responses;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using TeamServer.Models;
using TeamServer.Models.Extras;

namespace TeamServer.Services
{
    public class HardHatHub : Hub
    {

        //make a list of hub clients and add them on connect
        public static List<string> _clients = new List<string>();

        public override Task OnConnectedAsync()
        {
            if (!_clients.Contains(Context.ConnectionId))
            {
                //for a new connected client call the GetExistingManagerList function passing in the clients connection id
                var ManagerList = managerService._managers.Where(h => h.Type == manager.ManagerType.http || h.Type == manager.ManagerType.https).ToList();
                List<Httpmanager> httpManagersList = new();
                if(ManagerList != null)
                {
                    foreach (manager m in ManagerList)
                    {
                        httpManagersList.Add((Httpmanager)m);
                    }
                }
                var ManagerList2 = managerService._managers.Where(h => h.Type == manager.ManagerType.smb).ToList();
                List<SMBmanager> smbManagersList = new();
                if (ManagerList2 != null)
                {
                    foreach (manager m in ManagerList2)
                    {
                        smbManagersList.Add((SMBmanager)m);
                    }
                }

                GetExistingHttpManagers(httpManagersList, Context.ConnectionId);
                GetExistingTaskInfo(Engineer.previousTasks,Context.ConnectionId);
                _clients.Add(Context.ConnectionId);
            }
            return base.OnConnectedAsync();
        }

        public async Task<string> TriggerDownload(string OrginalPath)
        {
            var result =  System.IO.File.ReadAllBytes(OrginalPath);
            var resultString = Convert.ToBase64String(result);
            return resultString;
        }

        public async Task<string> HostFile(string file,string filename)
        {
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            // find the Engineer cs file and load it to a string so we can update it and then run the compiler function on it
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //split path at bin keyword
            string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
            //update each string in the array to replace \\ with the correct path seperator
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());

            var fileBytes = Convert.FromBase64String(file);
            System.IO.File.WriteAllBytes(pathSplit[0] + "wwwroot" + $"{allPlatformPathSeperator}{filename}", fileBytes);
            await AlertEventHistory(new HistoryEvent { Event = $"file {filename} uploaded to teamserver, can be accessed from any listeners address", Status = "success" });
            return "Success: Uploaded File";
        }

        public static  async Task CheckIn()
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;            
            await hubContext.Clients.All.SendAsync("NewEngineer");
        }

        public static async Task ShowEngineerTaskResponse(string engineerid, List<string> tasksIds)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("ShowEngineerTaskResponse", engineerid, tasksIds);
        }
        
        public static async Task UpdateOutgoingTaskDic(string engineerid, string taskId, string command, string arguments)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("UpdateOutgoingTaskDic", engineerid, taskId, command, arguments);
        }

        public static async Task UpdateManagerList(manager manager)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("UpdateManagerList", manager);
        }

        public static async Task GetExistingHttpManagers(List<Httpmanager> managers,string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.Client(clientId).SendAsync("GetExistingManagerList", managers);
        }
        
        public static async Task GetExistingSMBManagers(List<SMBmanager> managers, string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.Client(clientId).SendAsync("GetExistingManagerList", managers);
        }

        public static async Task GetExistingTaskInfo(Dictionary<string, List<EngineerTask>> TaskInfoDic,string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.Client(clientId).SendAsync("GetExistingTaskInfo", TaskInfoDic);
        }

        public static async Task AlertDownload(DownloadFile file)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AlertDownloadFile", file);
        }

        public static async Task AlertEventHistory(HistoryEvent histEvent)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AlertEventHistory", histEvent);
        }
        public static async Task AddCreds(List<Cred> creds)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AddCreds", creds);
        }
        public static async Task AddPsCommand(string pscmd)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AddPsCommand", pscmd);
        }

        //should handle sending the PivotProxy object to the clients 
        public static async Task AddPivotProxy(PivotProxy pivotProxy)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AddPivotProxy", pivotProxy);
        }

    }
}