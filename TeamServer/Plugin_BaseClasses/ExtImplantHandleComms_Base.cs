﻿using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Utilities;
using static SQLite.SQLite3;
using HardHatCore.TeamServer.Plugin_Management;

namespace HardHatCore.TeamServer.Plugin_BaseClasses
{
    [Export(typeof(IExtimplantHandleComms))]
    [ExportMetadata("Name", "Default")]
    public class ExtImplantHandleComms_Base : ControllerBase, IExtimplantHandleComms
    {

        public virtual async Task<ExtImplant_Base> GetCheckingInImplant(IExtImplantMetadata extImplantmetadata, HttpContext httpContext, IExtImplantService extImplantService_Base,string pluginName)
        {
            try
            {
                Httpmanager httpmanager = extImplantService_Base.GetImplantsManager(extImplantmetadata);
                if (httpmanager != null)
                {
                    SetHttpResponseHeaders(ref httpContext, httpmanager);
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
                if (httpContext.Request.Method.Equals("GET",StringComparison.CurrentCultureIgnoreCase))
                {
                    await HandleGetRequest(extImplant);
                }
                else if (httpContext.Request.Method.Equals("POST",StringComparison.CurrentCultureIgnoreCase))
                {
                    await HandlePostRequest(extImplant, extImplantService_Base, httpContext);
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

        public virtual async Task<IActionResult> RespondToImplant(ExtImplant_Base extImplant, IExtImplantService extImplantService_Base)
        {
            try
            {

               byte[] taskData = await ReturnImplantTasking(extImplant, extImplantService_Base);
                if (taskData != null && taskData.Length > 0)
                {
                    return new FileContentResult(taskData, "application/octet-stream");
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in tasking");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return BadRequest();
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
                        if (possibleTaskedImp.pendingTasks.Count() > 0)
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
                        //Console.WriteLine($"At {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss:ff")} implant {extImplant.Metadata.Id} tasked");
                        return encryptedC2MessageArray;
                    }
                    else if (extImplant.pendingTasks.Count() > 0)
                    {
                        byte[] encryptedC2MessageArray = await HandlePreProcAndPackageTasking(extImplant);
                        //Console.WriteLine($"At {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss:ff")} implant {extImplant.Metadata.Id} tasked");
                        return encryptedC2MessageArray;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (extImplant.pendingTasks.Count() > 0)
                {
                    byte[] encryptedC2MessageArray = await HandlePreProcAndPackageTasking(extImplant);
                    //Console.WriteLine($"At {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss:ff")} implant {extImplant.Metadata.Id} tasked");
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

        public virtual bool SetHttpResponseHeaders(ref HttpContext httpContext, Httpmanager httpmanager)
        {
            if (httpmanager == null)
            {
                foreach(var header in httpmanager.c2Profile.ResponseHeaders)
                {
                    var headerName = header.Substring(0, header.IndexOf(","));
                    var headerValue = header.Substring(header.IndexOf(",") + 1);
                    httpContext.Response.Headers.Add($"{headerName}", $"{headerValue}");
                }
                return true;
            }
            return false;
        }

        public virtual void CreateNewP2PPath(string implantId)
        {
            IExtimplantHandleComms.P2P_PathStorage.Add(implantId, new List<string>() { implantId });
        }

        public virtual async Task HandleGetRequest(ExtImplant_Base implant)
        {
            await HardHatHub.ImplantCheckIn(implant);
        }

        public virtual async Task HandlePostRequest(ExtImplant_Base implant, IExtImplantService extImpService_base, HttpContext copiedHttpContext)
        {
            try
            {
                //Console.WriteLine($"{DateTime.UtcNow} handling POST request");
                using var ms = new MemoryStream();
                await copiedHttpContext.Request.Body.CopyToAsync(ms);
                byte[] encryptedData = ms.ToArray();
                int implantIdLength = BitConverter.ToInt32(encryptedData, 0);
                string implantId = System.Text.Encoding.UTF8.GetString(encryptedData, 4, implantIdLength);
                byte[] encryptedDataWithoutId = encryptedData.Skip(4 + implantIdLength).ToArray();

                //set it to null so the GC can clean it asaps
                encryptedData = null;

                byte[] decryptedData;
                if (Encryption.UniqueTaskEncryptionKey.ContainsKey(implantId))
                {
                    decryptedData = extImpService_base.DecryptImplantTaskData(encryptedDataWithoutId, Encryption.UniqueTaskEncryptionKey[implantId]);
                }
                else
                {
                    decryptedData = extImpService_base.DecryptImplantTaskData(encryptedDataWithoutId, Encryption.UniversalTaskEncryptionKey);
                }
                if (decryptedData == null)
                {
                    decryptedData = extImpService_base.HandleP2PDataDecryption(implant, encryptedDataWithoutId);
                }

                IEnumerable<ExtImplantTaskResult_Base> processedTasks = await ProcessTaskResults(decryptedData.Deserialize<IEnumerable<ExtImplantTaskResult_Base>>(), implant);
                decryptedData = null;
                await SendTaskResults(implant, processedTasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in handling POST request");
                Console.WriteLine(ex.Message);
            }
           
        }

        public virtual async Task<IEnumerable<ExtImplantTaskResult_Base>> ProcessTaskResults(IEnumerable<ExtImplantTaskResult_Base> taskResults,ExtImplant_Base implant)
        {
            try
            {
                var extImplant_TaskPostProcess_Base = PluginService.GetImpPostProcPlugin(implant.ImplantType);
                List<ExtImplantTaskResult_Base> taskResultsToReturn = new List<ExtImplantTaskResult_Base>();
                foreach (var result in taskResults)
                {
                    if (result.ResponseType == ExtImplantTaskResponseType.DataChunk)
                    {
                        await ((ExtImplant_TaskPostProcess_Base)extImplant_TaskPostProcess_Base).HandleDataChunking(result);
                    }
                    //get the task from the implant
                    var implantTasks = await implant.GetTasks();
                    var task = implantTasks.FirstOrDefault(x => x.Id == result.Id);
                    
                    if (result.Status is ExtImplantTaskStatus.Running or ExtImplantTaskStatus.Complete)
                    {
                        if (task != null && extImplant_TaskPostProcess_Base.DetermineIfTaskPostProc(task) || result.Command.Equals("socksReceive", StringComparison.CurrentCultureIgnoreCase))
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
                        await DatabaseService.AsyncConnection.InsertAsync((ExtImplantTaskResult_DAO)result);
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
                        await Task.Run(async () =>
                        {
                            if (task is not null)
                            {
                                //Console.WriteLine($"removing task {result.Id} from implant: {result.Command}");
                                await implant.RemoveTask(task);
                            }
                        });
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

        public async Task SendTaskResults(ExtImplant_Base implant, IEnumerable<ExtImplantTaskResult_Base> results)
        {
            try
            {
                if(results == null && implant != null)
                {
                    await HardHatHub.ImplantCheckIn(implant);
                    return;
                }
                else if(results == null && implant == null)
                {
                    return;
                }
                List<string> implantTaskIds = new();
                foreach (var result in results)
                {
                    //if none of the entries in the list have the same id as the result, add it to the list
                    if (!implantTaskIds.Any(x => x == result.Id))
                    {
                        implantTaskIds.Add(result.Id);
                    }
                    if (result.ImplantId == implant.Metadata.Id)
                    {
                        await implant.AddTaskResult(result);
                    }
                    else
                    {
                        var imp = IExtImplantService._extImplants.Where(x => x.Metadata.Id == result.ImplantId).FirstOrDefault();
                        if (imp != null)
                        {
                            await imp.AddTaskResult(result);
                        }
                    }
                }
                //Console.WriteLine($"{DateTime.UtcNow} implant checking in");
                await HardHatHub.ImplantCheckIn(implant);
                //Console.WriteLine($"{DateTime.UtcNow} sending {implantTaskIds.Count} task results");
                await HardHatHub.SendTaskResults(implant, implantTaskIds);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        public virtual async Task<byte[]> HandlePreProcAndPackageTasking(ExtImplant_Base implant)
        {
            List<C2TaskMessage> c2TaskMessages = new List<C2TaskMessage>();
            var impTasks = implant.GetPendingTasks();

            bool IsTaskUpdatingKey = false;
            //gets the task preproc plugin for the implant type
            var extImplant_TaskPreProcess_Base = PluginService.GetImpPreProcPlugin(implant.ImplantType);
            //gets the implant service plugin for the implant type
            var svc_plugins = Plugin_Management.PluginService.pluginHub.implant_servicePlugins;
            var svc_plugin = svc_plugins.GetPluginEnumerableResult(implant.ImplantType);
            var extImplantService_Base = PluginService.GetImpServicePlugin(implant.ImplantType);

            foreach (var task in impTasks)
            {
                if(task.File is null)
                {
                    task.File = new byte[0];
                }
                await HardHatHub.AddTaskIdToPickedUpList(task.Id);
                if(extImplant_TaskPreProcess_Base.DetermineIfTaskPreProc(task))
                {
                    extImplant_TaskPreProcess_Base.PreProcessTask(task, implant);
                }
                if (task.Command.Equals("UpdateTaskKey", StringComparison.InvariantCultureIgnoreCase))
                {
                    IsTaskUpdatingKey = true;
                }
            }
            var taskArray = impTasks.Serialize();
            byte[] encryptedTaskArray;
            if (Encryption.UniqueTaskEncryptionKey.ContainsKey(implant.Metadata.Id) && !(IsTaskUpdatingKey))
            {
                encryptedTaskArray = extImplantService_Base.EncryptImplantTaskData(taskArray, Encryption.UniqueTaskEncryptionKey[implant.Metadata.Id]);
                //Console.WriteLine($"emcrypted task with implant unique key {Encryption.UniqueTaskEncryptionKey[implant.Metadata.Id]}");
            }
            else
            {
                encryptedTaskArray = extImplantService_Base.EncryptImplantTaskData(taskArray, Encryption.UniversalTaskEncryptionKey);
                //Console.WriteLine($"emcrypted task with universial key {Encryption.UniversalTaskEncryptionKey}");
            }
            if (!IExtimplantHandleComms.P2P_PathStorage.ContainsKey(implant.Metadata.Id))
            {
                IExtimplantHandleComms.P2P_PathStorage.Add(implant.Metadata.Id, new List<string>() { implant.Metadata.Id });
            }
            var c2Message = new C2TaskMessage() { PathMessage = IExtimplantHandleComms.P2P_PathStorage[implant.Metadata.Id], TaskData = encryptedTaskArray };
            c2TaskMessages.Add(c2Message);
            var enc_c2TaskMessages = extImplantService_Base.EncryptImplantTaskData(c2TaskMessages.Serialize(), Encryption.UniversialMessagePathKey);
            //Console.WriteLine($"Sending {c2TaskMessages.Count} C2 Message to {implant.Metadata.Id}, encrypted with key {Encryption.UniversialMessagePathKey}");
            return enc_c2TaskMessages;
        }

        public void CreateNewEncKeyAndTaskUpdate(ExtImplant_Base implant, IExtImplantService extImplantService_Base )
        {
            extImplantService_Base.GenerateUniqueEncryptionKeys(implant.Metadata.Id);
            ExtImplantTask_Base updateTaskKey = new ExtImplantTask_Base
            {
                Command = "UpdateTaskKey",
                Id = Guid.NewGuid().ToString(),
                Arguments = new Dictionary<string, string> { { "TaskKey", Encryption.UniqueTaskEncryptionKey[implant.Metadata.Id] } },
                File = null,
                IsBlocking = false
            };
            implant.QueueTask(updateTaskKey);
        }
    }
}
