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
using TeamServer.Models.InteractiveTerminal;
using Microsoft.AspNet.SignalR.Messaging;
using TeamServer.Models.Managers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
//using DynamicEngLoading;

namespace TeamServer.Services
{
    public class HardHatUser
    {
        public string SignalRClientId { get; set; } //track machine to send notifs to 
        public string Username { get; set; } //track user to send notifs to
        public List<string> Roles { get; set; } //track if the user is an admin or not
    }

    public class HardHatHub : Hub
    {

        //make a list of hub clients and add them on connect
        public static List<string> _clients = new List<string>();

        // public static List<string> TeamLeadIds = new List<string>();
        public static Dictionary<string, HardHatUser> SignalRUsers = new();

        public override Task OnConnectedAsync()
        {
            if (!_clients.Contains(Context.ConnectionId))
            {

                _clients.Add(Context.ConnectionId);
                //for a new connected client call the GetExistingManagerList function passing in the clients connection id
                var ManagerList = managerService._managers.Where(h => h.Type == manager.ManagerType.http || h.Type == manager.ManagerType.https).ToList();
                List<Httpmanager> httpManagersList = new();
                if (ManagerList != null)
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
                var ManagerList3 = managerService._managers.Where(h => h.Type == manager.ManagerType.tcp).ToList();
                List<TCPManager> tcpManagersList = new();
                if (ManagerList3 != null)
                {
                    foreach (manager m in ManagerList3)
                    {
                        tcpManagersList.Add((TCPManager)m);
                    }
                }
                //should be called when a client connects, list should be populated from task in Database Service on Ts startup
                GetExistingHttpManagers(httpManagersList, Context.ConnectionId);
                GetExistingSMBManagers(smbManagersList, Context.ConnectionId);
                GetExistingTCPManagers(tcpManagersList, Context.ConnectionId);
                GetExistingTaskInfo(Engineer.previousTasks, Context.ConnectionId);
                GetExistingCreds(Context.ConnectionId);
                GetExistingHistoryEvents(HistoryEvent.HistoryEventList, Context.ConnectionId);
                GetExistingDownloadedFiles(Context.ConnectionId);
                GetExistingUploadedFile(Context.ConnectionId);
                GetExistingPivotProxies(Context.ConnectionId);
                
            }
            return base.OnConnectedAsync();
        }

        //hub client invokable methods 
        #region ClientInvokableMethods
        public async Task<string> TriggerDownload(string OrginalPath)
        {
            var result = System.IO.File.ReadAllBytes(OrginalPath);
            var resultString = Convert.ToBase64String(result);
            await AlertEventHistory(new HistoryEvent { Event = $"Downloaded file {OrginalPath} from teamserver", Status = "success" });
            LoggingService.EventLogger.Information("Downloaded file {OrginalPath} from teamserver", OrginalPath);
            return resultString;
        }

        public async Task<string> HostFile(string file, string filename)
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
            LoggingService.EventLogger.Information("file {filename} uploaded to teamserver's wwwroot folder, can be accessed from any listeners address", filename);
            UploadedFile upload_file = new UploadedFile
            {
                Name = filename,
                SavedPath = pathSplit[0] + "wwwroot" + $"{allPlatformPathSeperator}{filename}",
                FileContent = fileBytes
            };
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync((UploadedFile_DAO)upload_file);
            UploadedFile.uploadedFileList.Add(upload_file);
            return "Success: Uploaded File";
        }

        public async Task<string> CancelRunningTask(string taskid, string eng)
        {
            Console.WriteLine("ts cancel running task called, creating task to send to implant");
            //make a new engineerTask for the engid and use the taskid in an argument with the key /TaskId
            EngineerTask task = new EngineerTask(Guid.NewGuid().ToString(), "cancelTask", new Dictionary<string, string>() { { "/TaskId", taskid } }, null, false);
            //find the engineer with the matching engid and add the task to the engineers task list
            Engineer engineer = EngineersController.engineerList.Where(e => e.engineerMetadata.Id == eng).FirstOrDefault();
            engineer.QueueTask(task);
            return task.Id;
        }

        public async Task<string> CreateReconCenterEntity(ReconCenterEntity reconCenterEntity)
        {
            //send the updated recon center entity to all clients
           // Console.WriteLine("teamserver invoking push reconCenter Entity");
            await Clients.All.SendAsync("PushReconCenterEntity", reconCenterEntity);
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync((ReconCenterEntity_DAO)reconCenterEntity);
            return "Success: Created Recon Center Entity";
        }

        public async Task<string> CreateReconCenterProperty(string entityName, ReconCenterEntity.ReconCenterEntityProperty reconCenterProperty)
        {
            //send the updated recon center property to all clients
            //Console.WriteLine("teamserver invoking push reconCenter Property");
            await Clients.All.SendAsync("PushReconCenterProperty", entityName, reconCenterProperty);
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            //var tempList = DatabaseService.AsyncConnection.Table<ReconCenterEntity_DAO>().Where(x => x.Name == entityName).ToListAsync();
            //foreach (var item in tempList.Result)
            //{
            //    Console.WriteLine(item.Name);
            //}
            ReconCenterEntity_DAO entityToUpdate = DatabaseService.AsyncConnection.Table<ReconCenterEntity_DAO>().Where(x => x.Name == entityName).ToListAsync().Result.FirstOrDefault();
            //we do this so the list can grow otherwise we would only be able to store one property in the database
            List<ReconCenterEntity.ReconCenterEntityProperty> entitiesProperties = entityToUpdate.Properties.ProDeserializeForDatabase<List<ReconCenterEntity.ReconCenterEntityProperty>>();
            entitiesProperties.Add(reconCenterProperty);
            entityToUpdate.Properties = entitiesProperties.ProSerialiseForDatabase(); // need to reseralize here
            DatabaseService.AsyncConnection.UpdateAsync(entityToUpdate);
            return "Success: Created Recon Center Property";
        }

        public async Task<string> UpdateReconCenterProperty(ReconCenterEntity.ReconCenterEntityProperty oldProperty, ReconCenterEntity.ReconCenterEntityProperty newProperty)
        {
            //send the updated recon center property to all clients
            //Console.WriteLine("teamserver invoking push reconCenter Property Update");
            await Clients.All.SendAsync("PushReconCenterPropertyUpdate", oldProperty, newProperty);
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            // need to update the stored ReconCenterProperty here, but it is stored as a seralized byte[] so would need to find the right entity, deseralize all of its properties, update the right one and then reseralize it 


            return "Success: Updated Recon Center Property";
        }

        public async Task<bool> CreateUser(string username, string passwordHash, byte[] salt,string role)
        {
            try
            {
                //create an instance of the UserInfo Class and set these properties, makes the Id a new guid , then make an instance of the UserStore and add the user to the store 
                UserInfo user = new UserInfo { Id = Guid.NewGuid().ToString(), UserName = username, NormalizedUserName = username.Normalize().ToUpperInvariant(), PasswordHash = passwordHash };
                UserStore userStore = new UserStore();
                var result = await userStore.CreateAsync(user, new CancellationToken());
                //Console.WriteLine($"{username}'s hashed password is {passwordHash}");
                await userStore.SetPasswordSaltAsync(user, salt);

                //created user needs to be give Operator role 
                await userStore.AddToRoleAsync(user, role, new CancellationToken());
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

        public async Task<bool> VerifyTokenUsernameExists(string username)
        {
            //create an instance of the UserStore and check if the user exists
            UserStore userStore = new UserStore();
            var user = await userStore.FindByNameAsync(username, new CancellationToken());
            if (user != null)
            {
                //if the user exists return true
                return true;
            }
            else
            {
                //if the user does not exist return false
                return false;
            }
        }

        public async Task<bool> CheckJWTExpiration(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                // Check if readable token (string is in a JWT format)
                var readableToken = handler.CanReadToken(token);
                if (!readableToken)
                {
                    Console.WriteLine("The token doesn't seem to be in a proper JWT format. Signing Out...");
                    return false;
                }
                var claimsPrincipal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Authentication.Configuration["Jwt:Key"])),
                    ValidIssuer = Authentication.Configuration["Jwt:Issuer"],
                    ValidAudience = Authentication.Configuration["Jwt:Issuer"],
                    ValidateLifetime = true, // check that the token is not expired
                    ClockSkew = TimeSpan.FromMinutes(5) // tolerance for the expiration date
                }, out var validatedToken);

                var jwtToken = validatedToken as JwtSecurityToken;
                var expiration = jwtToken.ValidTo; // Returns the expiration date
                if (expiration < DateTime.UtcNow)
                {
                    Console.WriteLine("The token is expired!  Signing Out...");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public async Task<byte[]> GetUserPasswordSalt(string username)
        {
            return await UserStore.GetUserPasswordSalt(username);

        }

        public async Task AddCred(Cred cred)
        {
            Cred.CredList.Add(cred);
            //check if database connection is not null 
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            else
            {
                //add the creds to the database
                DatabaseService.AsyncConnection.InsertAsync((Cred_DAO)cred);
            }

            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("AddCreds", new List<Cred> { cred });
        }

        public async Task PrettyLogs()
        {
            LoggingService.ToPretty();
        }

        public async Task CreateTerminalObject(InteractiveTerminalCommand command)
        {
            if (!InteractiveTerminalCommand.TerminalCommands.Contains(command))
            {
                LoggingService.TaskLogger.ForContext("Terminal Command", command, true).Information($"{command.Originator} executed terminal command {command.Command} @ {command.Timestamp}");
                InteractiveTerminalCommand.TerminalCommands.Add(command);
                TabView currenttab = TabView.Tabs.Where(x => x.TabId == command.TabId).ToList()[0];
                currenttab.Content.Add(command);
                UpdateTabContent(command);
            }
        }

        public async Task CreateTabViewObject(TabView tabview)
        {
            TabView.Tabs.Add(tabview);
        }

        public async Task<string> GetTerminalOutput(string commandId)
        {
            //find the matching command from the InteractiveTerminalCommand.TerminalCommands list if it does not exist return an empty string 
            InteractiveTerminalCommand command = InteractiveTerminalCommand.TerminalCommands.Where(x => x.Id == commandId).ToList()[0];
            if (command != null)
            {
                return command.Output;
            }
            else
            {
                return "";
            }
        }

        public async Task UpdateCommandOpsecLevelAndMitre(string commandName, HelpMenuItem.OpsecStatus opsecStatus,string mitreTechnique)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("UpdateCommandOpsecLevelAndMitre", commandName, opsecStatus,mitreTechnique);
            
        }

        public async Task AddNoteToImplant(string implantId, string note)
        {
              //find the implant in the list of implants 
            Engineer implant = EngineerService._engineers.Where(x => x.engineerMetadata.Id == implantId).ToList()[0];
            implant.Note = note;
            //update the implant in the database 
            await DatabaseService.AsyncConnection.UpdateAsync((Engineer_DAO)implant);
            //send the updated implant to the client 
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("UpdateImplantNote", implantId,note);
        }

        public async Task RegisterHardHatUserAfterSignin(string username)
        {
            //get the current signalr connection id
            string connectionId = Context.ConnectionId;
            //get the users current role 
            //first get the user object 
            UserStore userStore = new UserStore();
            var user = await userStore.FindByNameAsync(username, new CancellationToken());
            //get the users role
            List<string> roles = userStore.GetRolesAsync(user, new CancellationToken()).Result.ToList();
            
            HardHatUser newuser =  new HardHatUser { Username = username, SignalRClientId = connectionId, Roles = roles };
            if(SignalRUsers.ContainsKey(newuser.Username))
            {
                //update incase roles or connection id changed
                SignalRUsers[newuser.Username] = newuser;
            }
            else
            {
                SignalRUsers.Add(newuser.Username, newuser);
            }
        }

        public async Task UpdateTaskResponseSeenNotif(string username, string taskid, string engineerId)
        {
            //find the task in the list of tasks and add the username to the list of users who have seen the response
            //find the engineer in the list of engineers
            Engineer engineer = EngineerService._engineers.Where(x => x.engineerMetadata.Id == engineerId).ToList()[0];
            //find the task result in the list of task results
            EngineerTaskResult taskResult = engineer._taskResults.Where(x => x.Id == taskid).ToList()[0];
            if (!taskResult.UsersThatHaveReadResult.Contains(username))
            {
                taskResult.UsersThatHaveReadResult.Add(username);

                //make sure database connection is not null
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }

                //update the task result in the database
                int updated = await DatabaseService.AsyncConnection.UpdateAsync((EngineerTaskResult_DAO)taskResult);
            }
        }

        public async Task CreateorUpdateAlias(string username, Alias alias)
        {
            alias.Username = username;
            //check if the alias already exists using its id 
            if (Alias.savedAliases.Where(x => x.id == alias.id).ToList().Count > 0)
            {
                //update the alias 
                Alias.savedAliases.Where(x => x.id == alias.id).ToList()[0] = alias;
            }
            else
            {
                //add the alias to the list of aliases 
                Alias.savedAliases.Add(alias);
            }
            //make sure database connection is not null
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            //save the alias to the database
            await DatabaseService.AsyncConnection.InsertAsync((Alias_DAO)alias);
        }

        public async Task<List<Alias>> GetExistingAliases(string username)
        {
            //get the HardHatUser object with that id 
            List<Alias> aliasesToSend = new List<Alias>();
            aliasesToSend = Alias.savedAliases.Where(x => x.Username == username).ToList();
            return aliasesToSend;
        }

        //end of hub client invokable methods 
        #endregion

        //teamserver side invokable methods 
        #region teamserver_side_invoke_Methods
        public static async Task StoreTaskHeader(EngineerTask storeHeaderTask)
        {
            if(DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync((EngineerTask_DAO)storeHeaderTask);
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
        
        public static async Task GetExistingTCPManagers(List<TCPManager> managers, string clientId)
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
            DatabaseService.AsyncConnection.InsertAsync((DownloadFile_DAO)file);
            AlertEventHistory(new HistoryEvent() { Event = $"Downloaded File: {file.Name} from {file.Host} @ {file.downloadedTime} ", Status = "Success" });
            LoggingService.EventLogger.ForContext("File", file,true).Information($"Downloaded File: {file.Name} from {file.Host} @ {file.downloadedTime} ");
        }

        public static async Task AlertEventHistory(HistoryEvent histEvent)
        {
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            
            //add the creds to the database
            DatabaseService.AsyncConnection.InsertAsync((HistoryEvent_DAO)histEvent);
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
                    if (DatabaseService.AsyncConnection == null)
                    {
                        DatabaseService.ConnectDb();
                    }
                    else
                    {
                        //add the creds to the database
                        var credDAOs = creds.Select(cred => (Cred_DAO)cred);
                        DatabaseService.AsyncConnection.InsertAllAsync(credDAOs);
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

        public static async Task UpdateTabContent(InteractiveTerminalCommand command)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            await hubContext.Clients.All.SendAsync("UpdateTabContent", command);
        }
        
        public static async Task AddIOCFile(IOCFile iocFile)
        {
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
            
            //if the iocFile.Id matches any in the IOCFile.IOCFiles list then we got it from the db and can just send it to the clients 
            if (IOCFile.IOCFiles.Any(x => x.ID == iocFile.ID))
            {
                await hubContext.Clients.All.SendAsync("AddIOCFile", iocFile);
                return;
            }
            // otherwise this is new and add the ioc file to the database and tracking list and logging 
            DatabaseService.AsyncConnection.InsertAsync((IOCFIle_DAO)iocFile);
            IOCFile.IOCFiles.Add(iocFile);
            LoggingService.EventLogger.ForContext("IOC_File", iocFile,true).Information($"Uploaded File to target: {iocFile.Name} on host {iocFile.UploadedHost} to {iocFile.UploadedPath} @ {iocFile.Uploadtime} ");
            await hubContext.Clients.All.SendAsync("AddIOCFile", iocFile);
        }

        public static async Task AddCompiledImplant(CompiledImplant compImp)
        {
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;

            await hubContext.Clients.All.SendAsync("AddCompiledImplant", compImp);
            DatabaseService.AsyncConnection.InsertAsync((CompiledImplant_DAO)compImp);
            return;
        }
        //end of teamserver side invokable methods 
        #endregion
    }
}