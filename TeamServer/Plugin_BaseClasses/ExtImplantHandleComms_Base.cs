using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Shared;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HardHatCore.TeamServer.Plugin_BaseClasses
{
    public class ExtImplantHandleComms_Base : IExtimplantHandleComms
    {
        public string Name => "Default";
        public string Description => "Default asset communication class";
        
        //This region deals with responding to the implant, including packaging up the tasking and sending it back to the implant, checking encryption key updates, setting headers etc
        #region OutboundTrafficCode

        public virtual async Task<byte[]> RespondToImplant(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base)
        {
            try
            {
                byte[] taskData = await ReturnImplantTasking(extImplant, extImplantService_Base);
                if (taskData != null && taskData.Length > 0)
                {
                    return taskData;
                }
                else
                {
                    return Array.Empty<byte>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in tasking");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return Array.Empty<byte>();
            }
        }

        public virtual async Task<byte[]> ReturnImplantTasking(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base)
        {
            try
            {
                if (IExtimplantHandleComms.ParentToChildTracker.ContainsKey(extImplant.Metadata.Id))
                {
                    List<ExtImplant_Base> TaskingImps = new List<ExtImplant_Base>();
                    //find all the items in the P2Pstorage that have the current implant id as the value in its list

                    foreach (var item in IExtimplantHandleComms.P2P_PathStorage.Where(x => x.Value[0].Equals(extImplant.Metadata.Id)))
                    {
                        //check if the implant that is the item.Key has any pending tasks if so add it to the list of tasking imps
                        var possibleTaskedImp = extImplantService_Base.GetExtImplant(item.Key);
                        if (possibleTaskedImp.pendingTasks.Any() || possibleTaskedImp.assetNotifications.Any())
                        {
                            TaskingImps.Add(possibleTaskedImp);
                        }
                    }
                    //foreach item in the TaskingImps list, package the taskings and once they are all packaged, send them to the implant
                    if (TaskingImps.Count > 0)
                    {
                        byte[] encryptedC2MessageArray = new byte[0];
                        foreach (var taskedImp in TaskingImps)
                        {
                            byte[] encryptedC2MessageArrayTemp = await HandlePreProcAndPackageTasking(taskedImp);
                            encryptedC2MessageArray = encryptedC2MessageArray.Concat(encryptedC2MessageArrayTemp).ToArray();
                        }
                        return encryptedC2MessageArray;
                    }
                    else if (extImplant.pendingTasks.Any() || extImplant.assetNotifications.Any())
                    {
                        byte[] encryptedC2MessageArray = await HandlePreProcAndPackageTasking(extImplant);
                        return encryptedC2MessageArray;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (extImplant.pendingTasks.Any() || extImplant.assetNotifications.Any())
                {
                    byte[] encryptedC2MessageArray = await HandlePreProcAndPackageTasking(extImplant);
                    return encryptedC2MessageArray;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public virtual bool SetHttpResponseHeaders(ref HttpContext httpContext, HttpManager httpManager)
        {
            if (httpManager == null)
            {
                foreach (var header in httpManager.c2Profile.ResponseHeaders)
                {
                    var headerName = header.Substring(0, header.IndexOf(","));
                    var headerValue = header.Substring(header.IndexOf(",") + 1);
                    httpContext.Response.Headers.Add($"{headerName}", $"{headerValue}");
                }
                return true;
            }
            return false;
        }

        public virtual async Task<byte[]> HandlePreProcAndPackageTasking(ExtImplant_Base implant)
        {

            List<C2Message> c2TaskMessages = new List<C2Message>();
            var impTasks = await implant.GetPendingTasks();
            var assetNotifs = await implant.GetAssetNotifications();

            bool IsTaskUpdatingKey = false;
            //gets the task preproc plugin for the implant type
            var extImplant_TaskPreProcess_Base = PluginService.GetImpPreProcPlugin(implant.ImplantType);
            //gets the implant service plugin for the implant type
            var extImplantService_Base = PluginService.GetImpServicePlugin(implant.ImplantType);

            foreach (var task in impTasks)
            {
                if (task.File is null)
                {
                    task.File = Array.Empty<byte>();
                }
                await HardHatHub.AddTaskIdToPickedUpList(task.Id);
                if (extImplant_TaskPreProcess_Base.DetermineIfTaskPreProc(task))
                {
                    extImplant_TaskPreProcess_Base.PreProcessTask(task, implant);
                }
                if (task.Command.Equals("UpdateTaskKey", StringComparison.InvariantCultureIgnoreCase))
                {
                    IsTaskUpdatingKey = true;
                }
            }
            var taskArray = impTasks.Serialize();
            var assetNotifArray = assetNotifs.Serialize();
            byte[] encryptedTaskArray;
            byte[] encryptedAssetNotifArray;
            if (Encryption.UniqueTaskEncryptionKey.ContainsKey(implant.Metadata.Id) && !(IsTaskUpdatingKey))
            {
                if(taskArray.Length > 0) 
                {
                    Console.WriteLine($"encrypting task with key {Encryption.UniqueTaskEncryptionKey[implant.Metadata.Id]}");
                    encryptedTaskArray = extImplantService_Base.EncryptImplantTaskData(taskArray, Encryption.UniqueTaskEncryptionKey[implant.Metadata.Id]);
                }
                else
                {
                    encryptedTaskArray = Array.Empty<byte>();
                }
                if(assetNotifArray.Length > 0) 
                {
                    Console.WriteLine($"encrypting notif with key {Encryption.UniqueTaskEncryptionKey[implant.Metadata.Id]}");
                    encryptedAssetNotifArray = extImplantService_Base.EncryptImplantTaskData(assetNotifArray, Encryption.UniqueTaskEncryptionKey[implant.Metadata.Id]);
                }
                else
                {
                    encryptedAssetNotifArray = Array.Empty<byte>();
                }
            }
            else
            {
                Console.WriteLine($"encrypting task with universal key {Encryption.UniversalTaskEncryptionKey}");
                encryptedTaskArray = extImplantService_Base.EncryptImplantTaskData(taskArray, Encryption.UniversalTaskEncryptionKey);
                encryptedAssetNotifArray = extImplantService_Base.EncryptImplantTaskData(assetNotifArray, Encryption.UniversalTaskEncryptionKey);
            }
            if (!IExtimplantHandleComms.P2P_PathStorage.ContainsKey(implant.Metadata.Id))
            {
                IExtimplantHandleComms.P2P_PathStorage.TryAdd(implant.Metadata.Id, new List<string>() { implant.Metadata.Id });
            }
            var Taskc2Message = new C2Message() { PathMessage = IExtimplantHandleComms.P2P_PathStorage[implant.Metadata.Id], Data = encryptedTaskArray, MessageType = 1 };
            var AssetNotifc2Message = new C2Message() { PathMessage = IExtimplantHandleComms.P2P_PathStorage[implant.Metadata.Id], Data = encryptedAssetNotifArray, MessageType = 2 };
            c2TaskMessages.Add(AssetNotifc2Message);
            c2TaskMessages.Add(Taskc2Message);
#if DEBUG
            Console.WriteLine($"Sending {impTasks.Count()} tasks to {implant.Metadata.Id}");
            Console.WriteLine($"Sending {assetNotifs.Count()} notifs to {implant.Metadata.Id}");
            Console.WriteLine($"using encryption key {Encryption.UniversialMessageKey} on C2 messages");
#endif
            var enc_c2TaskMessages = Encryption.XorMessage((await c2TaskMessages.SerializeAsync()).Concat(extImplantService_Base.GetOutboundCustomMessage(implant)).ToArray(), Encryption.UniversialMessageKey);
            return enc_c2TaskMessages;
        }

        public void CreateNewEncKeyAndTaskUpdate(ExtImplant_Base implant, IExtImplantService extImplantService_Base)
        {
            extImplantService_Base.GenerateUniqueEncryptionKeys(implant.Metadata.Id);
            ExtImplantTask_Base updateTaskKey = new ExtImplantTask_Base
            {
                Id = Guid.NewGuid().ToString(),
                Command = "UpdateTaskKey",
                Arguments = new Dictionary<string, string> { { "TaskKey", Encryption.UniqueTaskEncryptionKey[implant.Metadata.Id] } },
                File = null,
                IsBlocking = false
            };
            implant.QueueTask(updateTaskKey);
        }


        #endregion

        //This region deals with the implant checking in, being processed, and any inbound messages being handled
        #region InboundTrafficCode
        public virtual string GetImplantType(IHeaderDictionary headers)
        {
            try
            {
                if (!headers.TryGetValue("Authorization", out var encryptedencodedMetadata))     //extracted as base64 due too TryGetValue
                {
                    Console.WriteLine("no authorization header");
                    return null;
                }
                // Console.WriteLine($"metadata encryption key is {Encryption.UniqueMetadataKey[decryptedImplantId]}");
                // cleans up the from out the `Authorization: Bearer METADATAHERE`  response 
                encryptedencodedMetadata = encryptedencodedMetadata.ToString().Remove(0, 7);
                //we need to extract the length and the implant name from the metadata it will be the bytes after the length bytes 
                //the length bytes will be the first 4 bytes of the metadata
                int length = BitConverter.ToInt32(Convert.FromBase64String(encryptedencodedMetadata).Take(4).ToArray(), 0);
                //the implant name will be the bytes after the length bytes
                string XORED_implantName = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedencodedMetadata).Skip(4).Take(length).ToArray());
                string implant_name = Encryption.DecryptImplantName(XORED_implantName);
                if (implant_name == "")
                {
#if DEBUG
                    Console.WriteLine("Failed to extract implant name, debugging, setting to Engineer");
                    implant_name = "Engineer";
#endif
                }
                return implant_name;

            }
            catch (Exception ex)
            {
                //DEBUG REMOVE
                Console.WriteLine("Failed to extract implant name");
                return "Engineer";
            }
        }

        //need to move this into the implant service so it can be overriden I have the name so i can find the plugins
        public virtual T ExtractMetadata<T>(IHeaderDictionary headers, string implant_name) where T : IExtImplantMetadata
        {
            try
            {
                if (!headers.TryGetValue("Authorization", out var encryptedencodedMetadataWithName))     //extracted as base64 due too TryGetValue
                {
                    Console.WriteLine("no authorization header");
                    return default;
                }
                // cleans up the from out the `Authorization: Bearer METADATAHERE`  response 
                encryptedencodedMetadataWithName = encryptedencodedMetadataWithName.ToString().Remove(0, 7);

                //remove the length bytes and the implant name bytes from the metadata
                int length = BitConverter.ToInt32(Convert.FromBase64String(encryptedencodedMetadataWithName).Take(4).ToArray(), 0);
                byte[] encryptedencodedMetadata = Convert.FromBase64String(encryptedencodedMetadataWithName).Skip(4 + length).ToArray();

                //find the correct plugin to decrypt the metadata
                var extImplantService_Base = PluginService.GetImpServicePlugin(implant_name);
                byte[] encodedMetadataArray = extImplantService_Base.DecryptImplantTaskData(encryptedencodedMetadata, Encryption.UniversialMetadataKey);
                if (encodedMetadataArray == null)
                {
                    return default;
                }
                // deserialise the metadata
                var metadata = encodedMetadataArray.Deserialize<T>();
                if (metadata == null)
                {
                    Console.WriteLine("Failed to extract metadata");
                    return default;
                }
                return metadata;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return default;
            }
        }

        public virtual async Task<ExtImplant_Base> GetCheckingInImplant(IExtImplantMetadata extImplantmetadata, HttpContext httpContext, IExtImplantService extImplantService_Base, string pluginName)
        {
            try
            {
                HttpManager httpManager = extImplantService_Base.GetImplantsManager(extImplantmetadata);
                if (httpManager != null)
                {
                    SetHttpResponseHeaders(ref httpContext, httpManager);
                }
                ExtImplant_Base extImplant = extImplantService_Base.GetExtImplant(extImplantmetadata.Id);
                if (extImplant is null)
                {
                    Console.WriteLine($"New implant {extImplantmetadata.Id} checking in");
                    extImplant = extImplantService_Base.InitImplantObj(extImplantmetadata, ref httpContext, pluginName);
                    extImplantService_Base.AddExtImplant(extImplant);
                    CreateNewP2PPath(extImplantmetadata.Id);
                    extImplantService_Base.LogImplantFirstCheckin(extImplant);
                    //This is where a new encryption key is made and a task is issued to the implant to update its encryption key
                    CreateNewEncKeyAndTaskUpdate(extImplant, extImplantService_Base);
                    extImplantService_Base.AddExtImplantToDatabase(extImplant);
                    await HardHatHub.InvokeNewCheckInWebhook(extImplant);
                }
                return extImplant;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public virtual async Task HandleImplantRequest(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base, HttpContext httpContext)
        {
            try
            {
                //updates the last checkin time
                extImplant.CheckIn();
                extImplantService_Base.UpdateImplantDBInfo(extImplant);
                if (httpContext.Request.Method.Equals("GET", StringComparison.CurrentCultureIgnoreCase))
                {
                    await HandleGetRequest(extImplant);
                }
                else if (httpContext.Request.Method.Equals("POST", StringComparison.CurrentCultureIgnoreCase))
                {
                    await HandlePostRequest(extImplant, httpContext);
                }
                else
                {
                    Console.WriteLine($"request type {httpContext.Request.Method} not currently supported");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public virtual void CreateNewP2PPath(string implantId)
        {
            IExtimplantHandleComms.P2P_PathStorage.TryAdd(implantId, new List<string>() { implantId });
        }

        public virtual async Task HandleGetRequest(ExtImplant_Base implant)
        {
            await HardHatHub.ImplantCheckIn(implant);
        }

        public virtual async Task HandlePostRequest(ExtImplant_Base implant, HttpContext copiedHttpContext)
        {
            try
            {
                //Console.WriteLine($"{DateTime.UtcNow} handling POST request");
                using var ms = new MemoryStream();
                await copiedHttpContext.Request.Body.CopyToAsync(ms);
                byte[] encryptedData = ms.ToArray();

                //after update this will be a C2Message array vs a task result array
                byte[] decryptedData = null;
                decryptedData = Encryption.XorMessage(encryptedData, Encryption.UniversialMessageKey);
                if (decryptedData is null)
                {
                    Console.WriteLine("Error decrypting message, ensure it was encrypted with the UniversialMessageKey");
                    return;
                }
                await HandleC2Messages(decryptedData.Deserialize<List<C2Message>>());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in handling POST request");
                Console.WriteLine(ex.Message);
            }

        }
        
        public async Task HandleC2Messages(List<C2Message> c2Messages)
        {
            foreach (C2Message message in c2Messages)
            {
                var defaultAssetService = PluginService.GetImpServicePlugin("Default");
                ExtImplant_Base asset = defaultAssetService.GetExtImplant(message.PathMessage[0]);
                var extImpService_base = PluginService.GetImpServicePlugin(asset.Name);
                byte[] decryptedData = Array.Empty<byte>();
                if (message.MessageType < 100)
                {
                    if (Encryption.UniqueTaskEncryptionKey.ContainsKey(message.PathMessage[0]))
                    {
                        #if DEBUG
                        Console.WriteLine($"Trying to decrypt message with implant unique key");
                        #endif
                        decryptedData = extImpService_base.DecryptImplantTaskData(message.Data, Encryption.UniqueTaskEncryptionKey[message.PathMessage[0]]);
                    }
                    else
                    {
                        #if DEBUG
                        Console.WriteLine($"Trying to decrypt message with implant Universal key");
                        #endif
                        decryptedData = extImpService_base.DecryptImplantTaskData(message.Data, Encryption.UniversalTaskEncryptionKey);
                    }
                }

                switch (message.MessageType)
                {
                    // this is a Task
                    case 1:
                    {
                        #if DEBUG
                        Console.WriteLine($"Received task message");
                        #endif
                        IEnumerable<ExtImplantTaskResult_Base> processedTasks = await HandleTaskResults(decryptedData.Deserialize<IEnumerable<ExtImplantTaskResult_Base>>(), asset);
                        await SendTaskResultsToClient(asset, processedTasks);
                        break;
                    }
                    //this is a notification
                    case 2:
                        await HandleAssetNotification(decryptedData.Deserialize<IEnumerable<AssetNotification>>(), asset);
                        break;
                    //reserved 
                    case <= 100 and >= 3:
                        Console.WriteLine($"Received a reserved message type {message.MessageType}");
                        Console.WriteLine("If this is meant to be a custom message type set MessageType to any number over 100, values between 3 and 100 are reserved");
                        break;
                    //this is for a custom message type, per implant
                    case > 100:
                        await HandleCustomMessageType(message, asset);
                        break;
                }
            }
        }

        public virtual async Task<IEnumerable<ExtImplantTaskResult_Base>> HandleTaskResults(IEnumerable<ExtImplantTaskResult_Base> taskResults, ExtImplant_Base implant)
        {
            try
            {
                Console.WriteLine($"got {taskResults.Count()} results for implant {implant.Metadata.Id}");
                var extImplant_TaskPostProcess_Base = PluginService.GetImpPostProcPlugin(implant.ImplantType);
                List<ExtImplantTaskResult_Base> taskResultsToReturn = new List<ExtImplantTaskResult_Base>();
                foreach (var result in taskResults)
                {
                    if (result.ResponseType == ExtImplantTaskResponseType.DataChunk)
                    {
                        await ((ExtImplant_TaskPostProcess_Base)extImplant_TaskPostProcess_Base).HandleDataChunking(result);
                    }
                    //get the task from the implant
                    var task = await implant.GetTask(result.Id);

                    if (result.Status is ExtImplantTaskStatus.Running or ExtImplantTaskStatus.Complete)
                    {
                        if (task != null && extImplant_TaskPostProcess_Base.DetermineIfTaskPostProc(task))
                        {
                            await extImplant_TaskPostProcess_Base.PostProcessTask(taskResults, result, implant, task);
                        }
                    }

                    if (!result.IsHidden)
                    {
                        taskResultsToReturn.Add(result);
                        if (DatabaseService.AsyncConnection == null)
                        {
                            DatabaseService.ConnectDb();
                        }
                        await implant.AddTaskResult(result);
                        await HardHatHub.AlertEventHistory(new HistoryEvent() { Event = $"Got response for task {result.Id}", Status = "Success" });

                        //off load the logging to a new thread so that the teamserver can continue the post request
                        Thread thread = new Thread(async () =>
                        {
                            string ResultValue;
                            if (result.ResponseType == ExtImplantTaskResponseType.String)
                            {
                                ResultValue = result.Result.Deserialize<MessageData>()?.Message ?? string.Empty;
                            }
                            else
                            {
                                ResultValue = LoggingService.HandleComplexTaskResultTypes(result);
                            }
                            LoggingService.TaskLogger.ForContext("Task Result", ResultValue.Split(new[] { "\r\n" }, StringSplitOptions.None)).ForContext("ImplantId", result.ImplantId).ForContext("Task Status", result.Status)
                            .ForContext("Command", task.TaskHeader).Information($"Got response for task {result.Id}");
                        });
                        thread.Start();
                    }
                    else
                    {
                        if (task is not null)
                        {
                            implant.RemoveTask(task);
                        }
                    }
                }
                return taskResultsToReturn;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
        
        public async Task HandleAssetNotification(IEnumerable<AssetNotification> notifs, ExtImplant_Base asset)
        {
            var extImplant_TaskPostProcess_Base = PluginService.GetAssetNotifService(asset.ImplantType);
            #if DEBUG
            Console.WriteLine($"Received {notifs.Count()} notifications from {asset.Metadata.Id}");
            #endif
            foreach (var notif in notifs)
            {
                await extImplant_TaskPostProcess_Base.ProcessAssetNotification(notif);
                //After we have done any Server side processing we need to check and forward the notification to the client
                if (notif.ForwardToClient)
                {
                    await HardHatHub.ForwardAssetNotification(notif);
                }
            }
        }

        public async Task HandleCustomMessageType(C2Message message, ExtImplant_Base asset)
        {
            #if DEBUG
            Console.WriteLine($"Received a custom message type {message.MessageType}");
            #endif
        }    

        public async Task SendTaskResultsToClient(ExtImplant_Base implant, IEnumerable<ExtImplantTaskResult_Base> results)
        {
            try
            {
                if (results == null && implant != null)
                {
                    await HardHatHub.ImplantCheckIn(implant);
                    return;
                }
                else if (results == null)
                {
                    return;
                }
                List<string> implantTaskIds = new();
                
                await HardHatHub.ImplantCheckIn(implant);
                //await HardHatHub.SendTaskResultIds(implant, implantTaskIds);
                await HardHatHub.SendTaskResults(implant, results.ToList());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        #endregion

    }
}
