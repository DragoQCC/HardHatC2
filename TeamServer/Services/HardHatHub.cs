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
using TeamServer.Controllers;
using TeamServer.Models;
using TeamServer.Models.Extras;
using TeamServer.Models.Database;
using TeamServer.Services.Extra;
using System.Threading;
using System.Text;
using TeamServer.Models.Dbstorage;
using TeamServer.Utilities;

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
                _clients.Add(Context.ConnectionId);
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
                //should be called when a client connects, list should be populated from task in Database Service on Ts startup
                GetExistingHttpManagers(httpManagersList, Context.ConnectionId);
                GetExistingTaskInfo(Engineer.previousTasks,Context.ConnectionId);
                GetExistingCreds(Context.ConnectionId);
                GetExistingHistoryEvents(HistoryEvent.HistoryEventList, Context.ConnectionId);
                GetExistingDownloadedFiles(Context.ConnectionId);
                GetExistingUploadedFile(Context.ConnectionId);
                GetExistingPivotProxies(Context.ConnectionId);
   
            }
            return base.OnConnectedAsync();
        }

        public async Task<string> TriggerDownload(string OrginalPath)
        {
            var result =  System.IO.File.ReadAllBytes(OrginalPath);
            var resultString = Convert.ToBase64String(result);
            await AlertEventHistory(new HistoryEvent { Event = $"Downloaded file {OrginalPath} from teamserver", Status = "success" });
            LoggingService.EventLogger.Information("Downloaded file {OrginalPath} from teamserver", OrginalPath);
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
            LoggingService.EventLogger.Information("file {filename} uploaded to teamserver's wwwroot folder, can be accessed from any listeners address",filename);
            UploadedFile upload_file = new UploadedFile
            {
                Name = filename,
                SavedPath = pathSplit[0] + "wwwroot" + $"{allPlatformPathSeperator}{filename}",
                FileContent = fileBytes
            };
            if(DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.Connection.Insert((UploadedFile_DAO)upload_file);
            UploadedFile.uploadedFileList.Add(upload_file);
            return "Success: Uploaded File";
        }

        public async Task CancelRunningTask(string taskid, string eng)
        {
            Console.WriteLine("ts cancel running task called, creating task to send to implant");
            //make a new engineerTask for the engid and use the taskid in an argument with the key /TaskId
            EngineerTask task = new EngineerTask(Guid.NewGuid().ToString(), "cancelTask", new Dictionary<string, string>() { { "/TaskId", taskid } }, null, false);
            //find the engineer with the matching engid and add the task to the engineers task list
            Engineer engineer = EngineersController.engineerList.Where(e => e.engineerMetadata.Id == eng).FirstOrDefault();
            engineer.QueueTask(task);
        }

        public async Task<string> CreateReconCenterEntity(ReconCenterEntity reconCenterEntity)
        {
            //send the updated recon center entity to all clients
            Console.WriteLine("teamserver invoking push reconCenter Entity");
            await Clients.All.SendAsync("PushReconCenterEntity", reconCenterEntity);
            if(DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.Connection.Insert((ReconCenterEntity_DAO)reconCenterEntity);
            return "Success: Created Recon Center Entity";
        }

        public async Task<string> CreateReconCenterProperty(string entityName, ReconCenterEntity.ReconCenterEntityProperty reconCenterProperty)
        {
            //send the updated recon center property to all clients
            Console.WriteLine("teamserver invoking push reconCenter Property");
            await Clients.All.SendAsync("PushReconCenterProperty",entityName, reconCenterProperty);
            if (DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            ReconCenterEntity_DAO entityToUpdate = DatabaseService.Connection.Table<ReconCenterEntity_DAO>().Where(x => x.Name == entityName).ToList()[0];
            //we do this so the list can grow otherwise we would only be able to store one property in the database
            List<ReconCenterEntity.ReconCenterEntityProperty> entitiesProperties = entityToUpdate.Properties.ProDeserialize<List<ReconCenterEntity.ReconCenterEntityProperty>>();
            entitiesProperties.Add(reconCenterProperty);
            entityToUpdate.Properties = entitiesProperties.ProSerialise(); // need to reseralize here
            DatabaseService.Connection.Update(entityToUpdate); 
            return "Success: Created Recon Center Property";
        }

        public async Task<string> UpdateReconCenterProperty(ReconCenterEntity.ReconCenterEntityProperty oldProperty, ReconCenterEntity.ReconCenterEntityProperty newProperty)
        {
            //send the updated recon center property to all clients
            Console.WriteLine("teamserver invoking push reconCenter Property Update");
            await Clients.All.SendAsync("PushReconCenterPropertyUpdate", oldProperty, newProperty);
            if (DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            // need to update the stored ReconCenterProperty here, but it is stored as a seralized byte[] so would need to find the right entity, deseralize all of its properties, update the right one and then reseralize it 


            return "Success: Updated Recon Center Property";
        }

        public async Task<bool> CreateUser(string username, string passwordHash,byte[] salt)
        {
            try
            {
                //create an instance of the UserInfo Class and set these properties, makes the Id a new guid , then make an instance of the UserStore and add the user to the store 
                UserInfo user = new UserInfo { Id = Guid.NewGuid().ToString(), UserName = username, NormalizedUserName = username.Normalize().ToUpperInvariant() , PasswordHash = passwordHash };
                UserStore userStore = new UserStore();
                var result = await userStore.CreateAsync(user, new CancellationToken());
                Console.WriteLine($"{username}'s hashed password is {passwordHash}");
                await userStore.SetPasswordSaltAsync(user, salt);

                //created user needs to be give Operator role 
                await userStore.AddToRoleAsync(user, "Operator", new CancellationToken());
                if (result.Succeeded)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public async Task<string> LoginUser(string username, string passwordHash)
        {
            //create an instance of the UserStore and check if the user exists
            UserStore userStore = new UserStore();
            var user = new UserInfo { UserName = username, PasswordHash = passwordHash, NormalizedUserName= username.Normalize().ToUpper() };
            Console.WriteLine($"{username}'s logging in hashed password is {passwordHash}");
            if (user != null)
            {
                string token = await Authentication.SignIn(user);
                //if the user exists check if the password hash matches the one in the store
                if (!string.IsNullOrEmpty(token))
                {
                    //sign was successful 
                    return token;
                }
                else
                {
                    //sign in failed
                    return null;
                }
            }
            else
            {
                //if the user does not exist return false
                return null;
            }
        }

        public async Task<byte[]> GetUserPasswordSalt(string username)
        {
          return await UserStore.GetUserPasswordSalt(username);
            
        }

        public async  Task AddCred(Cred cred)
        {
            Cred.CredList.Add(cred);
            //check if database connection is not null 
            if (DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            else
            {
                //add the creds to the database
                DatabaseService.Connection.Insert((Cred_DAO)cred);
            }

            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AddCreds", new List<Cred> { cred });
        }


        public async Task PrettyLogs()
        {
            LoggingService.ToPretty();
        }
        
        public static async Task StoreTaskHeader(EngineerTask storeHeaderTask)
        {
            if(DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.Connection.Insert((EngineerTask_DAO)storeHeaderTask);
        }

        public static async Task CheckIn(Engineer engineer)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;            
            await hubContext.Clients.All.SendAsync("CheckInEngineer",engineer);
        }

        public static async Task ShowEngineerTaskResponse(string engineerid, List<string> tasksIds)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("ShowEngineerTaskResponse", engineerid, tasksIds);
        }
        
        public static async Task UpdateOutgoingTaskDic(string engineerid, string taskId, string commandHeader)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("UpdateOutgoingTaskDic", engineerid, taskId, commandHeader);
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

        public static async Task GetExistingHistoryEvents(List<HistoryEvent> historyEvents, string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.Client(clientId).SendAsync("GetExistingHistoryEvents", historyEvents);
        }

        public static async Task GetExistingDownloadedFiles(string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.Client(clientId).SendAsync("GetExistingDownloadedFiles", DownloadFile.downloadFiles);
        }

        public static async Task GetExistingUploadedFile(string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.Client(clientId).SendAsync("GetExistingUploadedFiles", UploadedFile.uploadedFileList);
        }
        
        public static async Task GetExistingPivotProxies(string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.Client(clientId).SendAsync("GetExistingPivotProxies", PivotProxy.PivotProxyList);
        }

        public static async Task GetExistingCreds(string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.Client(clientId).SendAsync("GetExistingCreds", Cred.CredList);

        }

        public static async Task AlertDownload(DownloadFile file)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AlertDownloadFile", file);
            DownloadFile.downloadFiles.Add(file);
            DatabaseService.Connection.Insert((DownloadFile_DAO)file);
            AlertEventHistory(new HistoryEvent() { Event = $"Downloaded File: {file.Name} from {file.Host} @ {file.downloadedTime} ", Status = "Success" });
            LoggingService.EventLogger.ForContext("File", file,true).Information($"Downloaded File: {file.Name} from {file.Host} @ {file.downloadedTime} ");
        }

        public static async Task AlertEventHistory(HistoryEvent histEvent)
        {
            if (DatabaseService.Connection == null)
            {
                DatabaseService.ConnectDb();
            }
            
            //add the creds to the database
            DatabaseService.Connection.Insert((HistoryEvent_DAO)histEvent);
            HistoryEvent.HistoryEventList.Add(histEvent);
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AlertEventHistory", histEvent);
        }
       
        public static async Task AddCreds(List<Cred> creds, bool AddToDB)
        {
            try
            {
                
                if (AddToDB)
                {
                    //check if database connection is not null 
                    if (DatabaseService.Connection == null)
                    {
                        DatabaseService.ConnectDb();
                    }
                    else
                    {
                        //add the creds to the database
                        var credDAOs = creds.Select(cred => (Cred_DAO)cred);
                        DatabaseService.Connection.InsertAll(credDAOs);
                        //also add these creds to the in memory list
                        Cred.CredList.AddRange(creds);
                    }
                }
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Added {creds.Count()} Creds added to the cred store", Status = "success" });
                LoggingService.EventLogger.Information("Added {CredCount} creds added to the cred store",creds.Count());
                
                var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
                await hubContext.Clients.All.SendAsync("AddCreds", creds);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
           
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
        
        public static async Task AddTaskIdToPickedUpList(string taskId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AddTaskToPickedUpList", taskId);
        }

    }
}