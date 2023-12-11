using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using HardHatCore.ApiModels.Shared;
using System.Net.Http;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using System.Net;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Models.Database;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Models.InteractiveTerminal;
using HardHatCore.TeamServer.Models.Managers;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services.Extra;
using HardHatCore.TeamServer.Utilities;

namespace HardHatCore.TeamServer.Services
{
    public class HardHatHub : Hub<IHardHatHub>
    {

        //make a list of hub clients and add them on connect
        public static List<string> _clients = new List<string>();

        // public static List<string> TeamLeadIds = new List<string>();
        public static Dictionary<string, HardHatUser> SignalRUsers = new();

        public override Task OnConnectedAsync()
        {
            try
            {
                if (!_clients.Contains(Context.ConnectionId))
                {
                    Console.WriteLine("client connected to hub");
                    _clients.Add(Context.ConnectionId);
                    //for a new connected client call the GetExistingManagerList function passing in the clients connection id
                    var ManagerList = managerService._managers.Where(h => h.Type == ManagerType.http || h.Type == ManagerType.https).ToList();
                    List<Httpmanager> httpManagersList = new();
                    if (ManagerList != null)
                    {
                        foreach (manager m in ManagerList)
                        {
                            httpManagersList.Add((Httpmanager)m);
                        }
                    }
                    var ManagerList2 = managerService._managers.Where(h => h.Type == ManagerType.smb).ToList();
                    List<SMBmanager> smbManagersList = new();
                    if (ManagerList2 != null)
                    {
                        foreach (manager m in ManagerList2)
                        {
                            smbManagersList.Add((SMBmanager)m);
                        }
                    }
                    var ManagerList3 = managerService._managers.Where(h => h.Type == ManagerType.tcp).ToList();
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
                    foreach (ExtImplant_Base imp in IExtImplantService._extImplants)
                    {
                        UpdateOutgoingTaskDic(imp, imp.GetTasks().Result.ToList(), Context.ConnectionId);
                    }
                    GetExistingCreds(Context.ConnectionId);
                    GetExistingHistoryEvents(HistoryEvent.HistoryEventList, Context.ConnectionId);
                    GetExistingDownloadedFiles(Context.ConnectionId);
                    GetExistingUploadedFile(Context.ConnectionId);
                    GetExistingPivotProxies(Context.ConnectionId);

                }
                return base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return base.OnConnectedAsync();
            }

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
            // find the ExtImplant_Base cs file and load it to a string so we can update it and then run the compiler function on it
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
            //make a new implantTask for the engid and use the taskid in an argument with the key /TaskId
            ExtImplantTask_Base task = new ExtImplantTask_Base(Guid.NewGuid().ToString(), "cancelTask", new Dictionary<string, string>() { { "/TaskId", taskid } }, null, false, false, false, null, "", eng);
            //find the implant with the matching engid and add the task to the implants task list
            ExtImplant_Base implant = IExtImplantService._extImplants.Where(e => e.Metadata.Id == eng).FirstOrDefault();
            implant.QueueTask(task);
            return task.Id;
        }

        public async Task<string> CreateReconCenterEntity(ReconCenterEntity reconCenterEntity)
        {
            //send the updated recon center entity to all clients
            // Console.WriteLine("teamserver invoking push reconCenter Entity");
            await Clients.All.PushReconCenterEntity(reconCenterEntity);
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
            await Clients.All.PushReconCenterProperty(entityName, reconCenterProperty);
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
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
            await Clients.All.PushReconCenterPropertyUpdate(oldProperty, newProperty);
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            // need to update the stored ReconCenterProperty here, but it is stored as a seralized byte[] so would need to find the right entity, deseralize all of its properties, update the right one and then reseralize it 


            return "Success: Updated Recon Center Property";
        }

        public async Task<bool> CreateUser(string username, string passwordHash, byte[] salt, string role)
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

            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.AddCreds(new List<Cred> { cred });
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

        public async Task UpdateCommandOpsecLevelAndMitre(string commandName, HelpMenuItem.OpsecStatus opsecStatus, string mitreTechnique)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.UpdateCommandOpsecLevelAndMitre(commandName, opsecStatus, mitreTechnique);

        }

        public async Task AddNoteToImplant(string implantId, string note)
        {
            //find the implant in the list of implants 
            ExtImplant_Base implant = IExtImplantService._extImplants.Where(x => x.Metadata.Id == implantId).ToList()[0];
            implant.Note = note;
            //update the implant in the database 
            await DatabaseService.AsyncConnection.UpdateAsync((ExtImplant_DAO)implant);
            //send the updated implant to the client 
            IHubContext<HardHatHub, IHardHatHub> hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.UpdateImplantNote(implantId, note);
        }

        public async Task<HardHatUser> RegisterHardHatUserAfterSignin(string username)
        {
            //get the current signalr connection id
            string connectionId = Context.ConnectionId;
            //get the users current role 
            //first get the user object 
            UserStore userStore = new UserStore();
            var user = await userStore.FindByNameAsync(username, new CancellationToken());
            //get the users role
            List<string> roles = userStore.GetRolesAsync(user, new CancellationToken()).Result.ToList();

            HardHatUser newuser = new HardHatUser { Username = username, SignalRClientId = connectionId, Roles = roles };
            if (SignalRUsers.ContainsKey(newuser.Username))
            {
                //update incase roles or connection id changed
                SignalRUsers[newuser.Username] = newuser;
            }
            else
            {
                SignalRUsers.Add(newuser.Username, newuser);
            }
            return newuser;
        }

        public async Task UpdateTaskResponseSeenNotif(string username, string taskid, string implantId)
        {
            //find the task in the list of tasks and add the username to the list of users who have seen the response
            //find the implant in the list of implants
            ExtImplant_Base implant = IExtImplantService._extImplants.Where(x => x.Metadata.Id == implantId).ToList()[0];
            //find the task result in the list of task results
            ExtImplantTaskResult_Base? taskResult = implant.GetTaskResult(taskid);
            if (taskResult == null)
            {
                return;
            }
            if (taskResult.UsersThatHaveReadResult == null)
            {
                taskResult.UsersThatHaveReadResult = new List<string>();
            }
            if (!taskResult.UsersThatHaveReadResult.Contains(username))
            {
                taskResult.UsersThatHaveReadResult.Add(username);

                //make sure database connection is not null
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }

                //update the task result in the database
                int updated = await DatabaseService.AsyncConnection.UpdateAsync((ExtImplantTaskResult_DAO)taskResult);
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

        public async Task<bool> CreateOrUpdateWebhook(Webhook webhook)
        {
            if (Webhook.ExistingWebhooks.Where(x => x.Id == webhook.Id).ToList().Count > 0)
            {
                //update the webhook 
                Webhook.ExistingWebhooks.Where(x => x.Id == webhook.Id).ToList()[0] = webhook;
            }
            else
            {
                //add the webhook to the list of webhooks 
                Webhook.ExistingWebhooks.Add(webhook);
            }

            //make sure database connection is not null
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            //save the webhook to the database
            await DatabaseService.AsyncConnection.InsertAsync((Webhooks_DAO)webhook);

            return true;
        }

        public async Task<List<Webhook>> GetExistingWebhooks()
        {
            //get the existing webhooks and return them
            return Webhook.ExistingWebhooks;
        }

        public async Task<bool> RefreshTeamserverPlugins()
        {
            //refresh the plugins 
            PluginService.RefreshPlugins();
            return true;
        }

        public async Task NotifySharedNoteUpdate(string content)
        {
            //send the updated note to all users 
            IHubContext<HardHatHub, IHardHatHub> hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.SharedNoteUpdated(content);
        }

        //public async Task VNCsendHeartbeatToServer(string implantId, string vncSessionId, string IssuingUser)
        //{
        //    //takes in the mouse click from the client, and sends it to the implant
        //    var task = new ExtImplantTask_Base(Guid.NewGuid().ToString(), "HandleVNCClientInteraction",
        //        new Dictionary<string, string>()
        //        {
        //            { "/interactionEvent", VncInteractionEvent.View.ToString() },
        //            { "/sessionid", vncSessionId },
        //},
        //        null, false, true, true, null, IssuingUser);
        //    //find the implant in the list of implants
        //    ExtImplant_Base implant = ExtImplantService_Base._extImplants.Where(x => x.Metadata.Id == implantId).ToList()[0];
        //    implant.QueueTask(task);
        //}

        //public async Task VNCsendMouseClickToServer(double x, double y, long button, string implantId, string vncSessionId, string IssuingUser)
        //{
        //    //takes in the mouse click from the client, and sends it to the implant
        //    var task = new ExtImplantTask_Base(Guid.NewGuid().ToString(), "HandleVNCClientInteraction", 
        //        new Dictionary<string, string>() 
        //        { 
        //            { "/interactionEvent", VncInteractionEvent.MouseClick.ToString() },
        //            { "/x", x.ToString() },
        //            { "/y", y.ToString() },
        //            { "/button", button.ToString()},
        //            { "/sessionid", vncSessionId },
        //        }, 
        //        null, false, true, true, null, IssuingUser);
        //    //find the implant in the list of implants
        //    ExtImplant_Base implant = ExtImplantService_Base._extImplants.Where(x => x.Metadata.Id == implantId).ToList()[0];
        //    implant.QueueTask(task);
        //}

        //public async Task VNCsendMouseMoveToServer(double x, double y, string implantId, string vncSessionId, string IssuingUser)
        //{
        //      //takes in the mouse move from the client, and sends it to the implant
        //    var task = new ExtImplantTask_Base(Guid.NewGuid().ToString(), "HandleVNCClientInteraction",
        //                       new Dictionary<string, string>()
        //                       {
        //            { "/interactionEvent", VncInteractionEvent.MouseMove.ToString() },
        //            { "/x", x.ToString() },
        //            { "/y", y.ToString() },
        //            { "/sessionid", vncSessionId },
        //        },
        //    null, false, true, true, null, IssuingUser);
        //    //find the implant in the list of implants
        //    ExtImplant_Base implant = ExtImplantService_Base._extImplants.Where(x => x.Metadata.Id == implantId).ToList()[0];
        //    implant.QueueTask(task);
        //}

        //public async Task VNCSendTextToServer(string text, string implantId, string vncSessionId, string IssuingUser)
        //{
        //      //takes in the text from the client, and sends it to the implant
        //    var task = new ExtImplantTask_Base(Guid.NewGuid().ToString(), "HandleVNCClientInteraction",
        //                                      new Dictionary<string, string>()
        //                                      {
        //            { "/interactionEvent", VncInteractionEvent.KeySend.ToString() },
        //            { "/text", text },
        //            { "/sessionid", vncSessionId },
        //        },
        //          null, false, true, true, null, IssuingUser);
        //    //find the implant in the list of implants
        //    ExtImplant_Base implant = ExtImplantService_Base._extImplants.Where(x => x.Metadata.Id == implantId).ToList()[0];
        //    implant.QueueTask(task);
        //}

        //public async Task VNCsendGetClipboardToServer(string implantId, string vncSessionId)
        //{
        //      //takes in the text from the client, and sends it to the implant
        //    var task = new ExtImplantTask_Base(Guid.NewGuid().ToString(), "HandleVNCClientInteraction",
        //            new Dictionary<string, string>()
        //            {
        //                { "/interactionEvent", VncInteractionEvent.clipboard.ToString() },
        //                { "/sessionid", vncSessionId },
        //            },
        //            null, false, true, true, null, null);
        //    //find the implant in the list of implants
        //    ExtImplant_Base implant = ExtImplantService_Base._extImplants.Where(x => x.Metadata.Id == implantId).ToList()[0];
        //    implant.QueueTask(task);
        //}

        //public async Task VNCsendClipboardDataToServer(string clipboardData, string implantId, string vncSessionId,string IssuingUser)
        //{
        //        //takes in the text from the client, and sends it to the implant
        //    var task = new ExtImplantTask_Base(Guid.NewGuid().ToString(), "HandleVNCClientInteraction",
        //                                                                    new Dictionary<string, string>()
        //                                                                    {
        //            { "/interactionEvent", VncInteractionEvent.clipboardPaste.ToString() },
        //            { "/text", clipboardData },
        //            { "/sessionid", vncSessionId },
        //        },null, false, true, true, null, IssuingUser);
        //    //find the implant in the list of implants
        //    ExtImplant_Base implant = ExtImplantService_Base._extImplants.Where(x => x.Metadata.Id == implantId).ToList()[0];
        //    implant.QueueTask(task);
        //}

        public async Task<List<string>> GetBindableAddressesOnServer()
        {
            List<string> bindableAddresses = new List<string>();
            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                bindableAddresses.Add(ip.ToString());
            }
            return bindableAddresses;

        }

        #endregion
        //end of hub client invokable methods

        //teamserver side invokable methods 
        #region teamserver_side_invoke_Methods
        public static async Task StoreTaskHeader(ExtImplantTask_Base mytask)
        {
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync((ExtImplantTask_DAO)mytask);
        }

        //public static async Task CheckIn(ExtImplant_Base implant)
        //{
        //    var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, ITeamServer>)) as IHubContext<HardHatHub, ITeamServer>;            
        //    await hubContext.Clients.All.SendAsync("CheckInExtImplant_Base",implant);
        //}

        public static async Task ImplantCheckIn(ExtImplant_Base implant)
        {
            try
            {
                var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
                await hubContext.Clients.All.CheckInImplant(implant);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        //public static async Task ShowExtImplant_BaseTaskResponse(string implantid, List<string> tasksIds)
        //{
        //    var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub>)) as IHubContext<HardHatHub>;
        //    await hubContext.Clients.All.SendAsync("ShowExtImplant_BaseTaskResponse", implantid, tasksIds);
        //}

        public static async Task UpdateOutgoingTaskDic(ExtImplant_Base implant, List<ExtImplantTask_Base> task, string connectionId)
        {
            //should execute whenever a task is queued
            if (String.IsNullOrEmpty(connectionId))
            {
                var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
                await hubContext.Clients.All.UpdateOutgoingTaskDic(implant, task);
            }
            //should execute whenever a new client connects for the first time and needs to be updated with the current task list
            else
            {
                var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
                await hubContext.Clients.Client(connectionId).UpdateOutgoingTaskDic(implant, task);
            }
        }

        public static async Task UpdateManagerList(manager manager)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.UpdateManagerList(manager);
        }

        public static async Task GetExistingHttpManagers(List<Httpmanager> managers, string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingManagerList(managers);
        }

        public static async Task GetExistingSMBManagers(List<SMBmanager> managers, string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingManagerList(managers);
        }

        public static async Task GetExistingTCPManagers(List<TCPManager> managers, string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingManagerList(managers);
        }

        public static async Task GetExistingTaskInfo(ExtImplant_Base implant, List<ExtImplantTask_Base> results, string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingTaskInfo(implant, results);
        }

        public static async Task GetExistingHistoryEvents(List<HistoryEvent> historyEvents, string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingHistoryEvents(historyEvents);
        }

        public static async Task GetExistingDownloadedFiles(string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingDownloadedFiles(DownloadFile.downloadFiles);
        }

        public static async Task GetExistingUploadedFile(string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingUploadedFiles(UploadedFile.uploadedFileList);
        }

        public static async Task GetExistingPivotProxies(string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingPivotProxies(PivotProxy.PivotProxyList);
        }

        public static async Task GetExistingCreds(string clientId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.Client(clientId).GetExistingCreds(Cred.CredList);

        }

        public static async Task AlertDownload(DownloadFile file)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.AlertDownloadFile(file);
            DownloadFile.downloadFiles.Add(file);
            DatabaseService.AsyncConnection.InsertAsync((DownloadFile_DAO)file);
            AlertEventHistory(new HistoryEvent() { Event = $"Downloaded File: {file.Name} from {file.Host} @ {file.downloadedTime} ", Status = "Success" });
            LoggingService.EventLogger.ForContext("File", file, true).Information($"Downloaded File: {file.Name} from {file.Host} @ {file.downloadedTime} ");
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
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.AlertEventHistory(histEvent);
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
                LoggingService.EventLogger.Information("Added {CredCount} creds added to the cred store", creds.Count());

                var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
                await hubContext.Clients.All.AddCreds(creds);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }

        public static async Task AddPsCommand(string pscmd)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.AddPsCommand(pscmd);
        }

        //should handle sending the PivotProxy object to the clients 
        public static async Task AddPivotProxy(PivotProxy pivotProxy)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.AddPivotProxy(pivotProxy);
        }

        public static async Task AddTaskIdToPickedUpList(string taskId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.AddTaskToPickedUpList(taskId);
        }

        public static async Task UpdateTabContent(InteractiveTerminalCommand command)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.UpdateTabContent(command);
        }

        public static async Task AddIOCFile(IOCFile iocFile)
        {
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;

            //if the iocFile.Id matches any in the IOCFile.IOCFiles list then we got it from the db and can just send it to the clients 
            if (IOCFile.IOCFiles.Any(x => x.ID == iocFile.ID))
            {
                await hubContext.Clients.All.AddIOCFile(iocFile);
                return;
            }
            // otherwise this is new and add the ioc file to the database and tracking list and logging 
            DatabaseService.AsyncConnection.InsertAsync((IOCFIle_DAO)iocFile);
            IOCFile.IOCFiles.Add(iocFile);
            LoggingService.EventLogger.ForContext("IOC_File", iocFile, true).Information($"Uploaded File to target: {iocFile.Name} on host {iocFile.UploadedHost} to {iocFile.UploadedPath} @ {iocFile.Uploadtime} ");
            await hubContext.Clients.All.AddIOCFile(iocFile);
        }

        public static async Task AddCompiledImplant(CompiledImplant compImp)
        {
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;

            await hubContext.Clients.All.AddCompiledImplant(compImp);
            DatabaseService.AsyncConnection.InsertAsync((CompiledImplant_DAO)compImp);
            return;
        }

        public static async Task InvokeNewCheckInWebhook(ExtImplant_Base implant)
        {
            //find all the webhooks that have the same event type and invoke them
            var webhooks = Webhook.ExistingWebhooks.Where(x => x.WebHookDataOption.Equals(Webhook.WebHookDataOptions.NewCheckIn));

            foreach (var hook in webhooks)
            {
                string temp_data = hook.DataTemplate;
                string temp_template = hook.Template;
                //replace the properties with the implant object properties
                foreach (var prop in implant.GetType().GetProperties())
                {
                    if (prop.GetValue(implant) != null)
                    {
                        temp_data = temp_data.Replace($"{{{{REPLACE_{prop.Name.ToUpper()}}}}}", prop.GetValue(implant).ToString());
                    }
                    else
                    {
                        temp_data = temp_data.Replace($"{{{{REPLACE_{prop.Name.ToUpper()}}}}}", "null");
                    }
                }
                foreach (var prop in implant.Metadata.GetType().GetProperties())
                {
                    if (prop.GetValue(implant.Metadata) != null)
                    {
                        temp_data = temp_data.Replace($"{{{{REPLACE_{prop.Name.ToUpper()}}}}}", prop.GetValue(implant.Metadata).ToString());
                    }
                    else
                    {
                        temp_data = temp_data.Replace($"{{{{REPLACE_{prop.Name.ToUpper()}}}}}", "null");
                    }
                }
                //update the webhook template title to be event type so NewCheckIn 
                temp_template = temp_template.Replace("{{REPLACE_TITLE}}", Webhook.WebHookDataOptions.NewCheckIn.ToString());

                //repalce the {{REPLACE_WEBHOOKCONTENT}} with the webhookContent
                temp_template = temp_template.Replace("{{REPLACE_WEBHOOKCONTENT}}", temp_data);

                //send the webhook as a post request using the http client 
                var client = new HttpClient();
                var response = await client.PostAsync(hook.Url, new StringContent(temp_template, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Webhook message sent successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to send webhook message: " + response.StatusCode);
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                }

            }
        }
        public static async Task InvokeNewCheckInWebhook(IExtImplant implant)
        {
            //find all the webhooks that have the same event type and invoke them
            var webhooks = Webhook.ExistingWebhooks.Where(x => x.WebHookDataOption.Equals(Webhook.WebHookDataOptions.NewCheckIn));

            foreach (var hook in webhooks)
            {
                string temp_data = hook.DataTemplate;
                string temp_template = hook.Template;
                //replace the properties with the implant object properties
                foreach (var prop in implant.GetType().GetProperties())
                {
                    if (prop.GetValue(implant) != null)
                    {
                        temp_data = temp_data.Replace($"{{{{REPLACE_{prop.Name.ToUpper()}}}}}", prop.GetValue(implant).ToString());
                    }
                    else
                    {
                        temp_data = temp_data.Replace($"{{{{REPLACE_{prop.Name.ToUpper()}}}}}", "null");
                    }
                }
                foreach (var prop in implant.Metadata.GetType().GetProperties())
                {
                    if (prop.GetValue(implant.Metadata) != null)
                    {
                        temp_data = temp_data.Replace($"{{{{REPLACE_{prop.Name.ToUpper()}}}}}", prop.GetValue(implant.Metadata).ToString());
                    }
                    else
                    {
                        temp_data = temp_data.Replace($"{{{{REPLACE_{prop.Name.ToUpper()}}}}}", "null");
                    }
                }
                //update the webhook template title to be event type so NewCheckIn 
                temp_template = temp_template.Replace("{{REPLACE_TITLE}}", Webhook.WebHookDataOptions.NewCheckIn.ToString());

                //repalce the {{REPLACE_WEBHOOKCONTENT}} with the webhookContent
                temp_template = temp_template.Replace("{{REPLACE_WEBHOOKCONTENT}}", temp_data);

                //send the webhook as a post request using the http client 
                var client = new HttpClient();
                var response = await client.PostAsync(hook.Url, new StringContent(temp_template, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Webhook message sent successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to send webhook message: " + response.StatusCode);
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                }

            }
        }

        public static async Task SendTaskResults(ExtImplant_Base implant, List<string> implantTaskIds)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.SendTaskResults(implant, implantTaskIds);
        }

        public static async Task<bool> CreateUserTS(string username, string password, string role)
        {
            try
            {
                //create a new salt and hash the password
                string passwordHash = Hash.HashPassword(password, out byte[] salt);
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

        public static async Task NotifyTaskDeletion(string implantId, string taskId)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.NotifyTaskDeletion(implantId, taskId);
        }

        public static async Task AddVNCInteractionResponse(VncInteractionResponse response, VNCSessionMetadata vncSessionMetadata)
        {
            var hubContext = Program.WebHost.Services.GetService(typeof(IHubContext<HardHatHub, IHardHatHub>)) as IHubContext<HardHatHub, IHardHatHub>;
            await hubContext.Clients.All.NotifyVNCInteractionResponse(response, vncSessionMetadata);
        }

        #endregion
        //end of teamserver side invokable methods 
    }
}