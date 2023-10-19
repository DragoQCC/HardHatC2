using HardHatCore.ApiModels.Requests;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HardHatCore.TeamServer.Models;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models.Managers;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Authorization;
using HardHatCore.ApiModels.Shared;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;

//using DynamicEngLoading;

namespace HardHatCore.TeamServer.Controllers
{
	[Authorize(Roles ="Operator,TeamLead")]
	[ApiController]
	[Route("[controller]")]
	public class ImplantsController : ControllerBase
	{
		// class member variables 
        public static List<IExtImplant> implantList = new();


		
		// API methods and functions 
		[HttpGet(Name = "RetrieveAllImplants")]
		public IActionResult GetImplants()
		{
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var Implants = _ImplantSvc.GetExtImplants();
			return Ok(Implants);
		}

		[HttpGet("{ImplantId}", Name = "RetrieveImplant")]
		public IActionResult GetImplant(string ImplantId)
		{
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var implant = _ImplantSvc.GetExtImplant(ImplantId);
            if (implant is null)
			{ 
				return NotFound(); 
			}
			return Ok(implant);
		}

		[HttpGet("{ImplantId}/taskresults/{taskId}", Name = "RetrieveImplantTaskResults")]
		public IActionResult GetTaskResult(string ImplantId, string taskId)
		{
			var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
			var implant = _ImplantSvc.GetExtImplant(ImplantId);
			if (implant is null)
			{
				Console.WriteLine("Implant not found when trying to get task results");
				return NotFound("Implant not found");
			}
			var result = implant.GetTaskResult(taskId);
			if (result is null)
			{
                Console.WriteLine("Task not found when trying to get task results");
                return NotFound(result);
			}
			return Ok(result);
			
        }

		[HttpGet("{ImplantId}/taskresults", Name = "RetrieveAllImplantTaskResults")]
		public IActionResult GetTaskResults(string ImplantId)
		{
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var Implant = _ImplantSvc.GetExtImplant(ImplantId);
			if (Implant is null) return NotFound("Implant not found");

			var results = Implant.GetTaskResults().Result;
			return Ok(results);
		}

		//get all the implant tasks 
		[HttpGet("{ImplantId}/tasks", Name = "RetrieveAllImplantTasks")]
		public async Task<IActionResult> GetTasks(string ImplantId)
		{
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var Implant = _ImplantSvc.GetExtImplant(ImplantId);
            if (Implant is null) return NotFound("Implant not found");

			var tasks = await Implant.GetTasks();
            return Ok(tasks);
        }

		[HttpPost("{ImplantId}", Name = "TaskImplant")]
		public IActionResult TaskImplant(string implantId, [FromBody] TaskExtImplantRequest_Base request)
		{
			if (request.TaskingExtras.TryGetValue("PostExImplantRequest", out var requestObject))
			{
                ExtImplantCreateRequest_Base implantSpwnRequest = requestObject.Deserialize<ExtImplantCreateRequest_Base>();
				if (request.Arguments.TryGetValue("/method", out string method))
				{
					method = method.Trim();
					if (method.Equals("psexec", StringComparison.CurrentCultureIgnoreCase))
					{
						implantSpwnRequest.complieType = ImpCompileType.serviceexe;
					}
					else
					{
						implantSpwnRequest.complieType = ImpCompileType.exe;
					}
				}
				else
				{
					implantSpwnRequest.complieType = ImpCompileType.exe;

				}
                bool isCreated = false;
				string result_message = null;
				var svc_plugins = Plugin_Management.PluginService.pluginHub.implant_servicePlugins;
				var svc_plugin = svc_plugins.GetPluginEnumerableResult(implantSpwnRequest.implantType);

                isCreated = svc_plugin.Value.CreateExtImplant(implantSpwnRequest, out result_message);				
				if (!isCreated)
				{
					Console.WriteLine("error in post ex command implant creation : " + result_message);
					return BadRequest(result_message);
				}
			}
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var implant = _ImplantSvc.GetExtImplant(implantId);
			if (implant is null) return NotFound();

			var task = new ExtImplantTask_Base(request.taskID, request.Command, request.Arguments, request.File, request.IsBlocking,request.RequiresPreProc, request.RequiresPostProc, request.TaskHeader, request.IssuingUser,implantId);

			if (!implant.QueueTask(task).Result)
			{
				return NotFound();
			}
            var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
            var path = $"{root}/tasks/{task.Id}";
            //if task.Arguments is not null, turn the dictionary into a string like /key1 value1 /key2 value2 etc
            var args = task.Arguments is null ? "" : string.Join(" ", task.Arguments.Select(kvp => $"{kvp.Key} {kvp.Value}"));
            if (String.IsNullOrEmpty(request.TaskHeader))
            {
                task.TaskHeader = $"({DateTime.UtcNow}) Implant instructed to {task.Command + " " + args}\n";
            }
            HardHatHub.StoreTaskHeader(task);
            // add the implantId as the key with a new list of ImplantTask to the Implant.Previous task Dictionary and add the task to the list of tasks for the implant
            ExtImplantTask_Base LoggingTask = new ExtImplantTask_Base() { Id = task.Id, Command = task.Command, Arguments = task.Arguments, File = null, IsBlocking = task.IsBlocking, 
                RequiresPostProc = task.RequiresPostProc, RequiresPreProc = task.RequiresPreProc, TaskHeader = task.TaskHeader };

            HardHatHub.UpdateOutgoingTaskDic(implant, new List<ExtImplantTask_Base>() { task }, "");
            HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"task {task.Command} {args} queued for execution", Status = "info" });
            LoggingService.TaskLogger.ForContext("Task", LoggingTask, true).ForContext("Implant_Id", implantId).Information($"task {task.Command} queued for execution");
            return Created(path, task);
        }

		//using the implant id and task id delete the task from the implant
		[HttpDelete("{ImplantId}/tasks/{taskId}", Name = "DeleteTask")]
		public async Task<IActionResult> DeleteTask(string ImplantId, string taskId)
		{
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var Implant = _ImplantSvc.GetExtImplant(ImplantId);
            if (Implant is null) return NotFound("Implant not found");

			var task =await  Implant.GetTask(taskId);
            if (task is null) return NotFound("Task not found");

            if (await Implant.RemoveTask(task))
			{
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"task {task.Command} {task.Arguments} deleted", Status = "info" });
                LoggingService.TaskLogger.ForContext("Task", task, true).ForContext("Implant_Id", ImplantId).Information($"task {task.Command} {task.Arguments} deleted");
				HardHatHub.NotifyTaskDeletion(Implant.Metadata.Id, task.Id);
                //connect to db and delete task
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }
				//check if it is also in the task result dao table and delete it from there as well 
				var taskResult = DatabaseService.AsyncConnection.Table<ExtImplantTaskResult_DAO>().Where(t => t.TaskId == task.Id).FirstOrDefaultAsync();
				if (taskResult != null)
				{
					  DatabaseService.AsyncConnection.DeleteAsync(taskResult);
				}
                return Ok();
            }
            return NotFound();
        }

		//using the implant id delete all tasks from the implant
		[HttpDelete("{ImplantId}/tasks", Name = "DeleteAllTasks")]
		public IActionResult DeleteAllTasks(string ImplantId, [FromBody] string confirmMessage)
		{
			if (!confirmMessage.Equals("Delete all results from this implant",StringComparison.CurrentCultureIgnoreCase))
			{
				return BadRequest("Please confirm you want to delete all tasks by sending the string 'Delete all results from this implant' in the body of the request");
			}
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var Implant = _ImplantSvc.GetExtImplant(ImplantId);
            if (Implant is null) return NotFound("Implant not found");

			var taskList = Implant.GetTasks().Result;

            if (taskList.Any())
			{
                foreach (var task in taskList)
				{
                    HardHatHub.NotifyTaskDeletion(Implant.Metadata.Id, task.Id);
                    //connect to db and delete task
                    if (DatabaseService.AsyncConnection == null)
					{
                        DatabaseService.ConnectDb();
                    }
                    DatabaseService.AsyncConnection.DeleteAsync((ExtImplantTask_DAO)task);
                    //check if it is also in the task result dao table and delete it from there as well 
                    var taskResult = DatabaseService.AsyncConnection.Table<ExtImplantTaskResult_DAO>().Where(t => t.TaskId == task.Id).FirstOrDefaultAsync();
                    if (taskResult != null)
					{
                        DatabaseService.AsyncConnection.DeleteAsync(taskResult);
                    }
                }
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"all tasks deleted", Status = "info" });
                LoggingService.TaskLogger.ForContext("Implant_Id", ImplantId).Information($"all tasks deleted");
                return Ok();
            }
            return NotFound();
        }

		//delete all tasks from all implants
		[HttpDelete("tasks", Name = "DeleteAllTasksFromAllImplants")]
		public IActionResult DeleteAllTasksFromAllImplants([FromBody] string confirmMessage)
		{
            if (!confirmMessage.Equals("Delete all results from all implants", StringComparison.CurrentCultureIgnoreCase))
			{
                return BadRequest("Please confirm you want to delete all tasks by sending the string 'Delete all results from all implants' in the body of the request");
            }
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            List<ExtImplant_Base> ImplantList = _ImplantSvc.GetExtImplants().ToList();

            if (ImplantList.Count > 0)
			{
                foreach (var Implant in ImplantList)
				{
                    var taskList = Implant.GetTasks().Result;
                    if (taskList.Any())
					{
                        foreach (var task in taskList)
                        {
                            HardHatHub.NotifyTaskDeletion(Implant.Metadata.Id, task.Id);
                            //connect to db and delete task
                            if (DatabaseService.AsyncConnection == null)
                            {
                                DatabaseService.ConnectDb();
                            }
                            DatabaseService.AsyncConnection.DeleteAsync((ExtImplantTask_DAO)task);
                            //check if it is also in the task result dao table and delete it from there as well 
                            var taskResult = DatabaseService.AsyncConnection.Table<ExtImplantTaskResult_DAO>().Where(t => t.TaskId == task.Id).FirstOrDefaultAsync();
                            if (taskResult != null)
                            {
                                DatabaseService.AsyncConnection.DeleteAsync(taskResult);
                            }
                        }
                    }
                }
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"all tasks deleted", Status = "info" });
                LoggingService.TaskLogger.Information($"all tasks deleted");
                return Ok();
            }
            return NotFound();
        }
        
		[HttpPost(Name = "CreateImplant")]
		public IActionResult CreateImplant([FromBody] ExtImplantCreateRequest_Base implantSpwnRequest)
		{
			try
			{
                bool isCreated = false;
                string result_message = null;
                var svc_plugins = Plugin_Management.PluginService.pluginHub.implant_servicePlugins;
                var svc_plugin = svc_plugins.GetPluginEnumerableResult(implantSpwnRequest.implantType);

                isCreated = svc_plugin.Value.CreateExtImplant(implantSpwnRequest, out result_message);
                if (isCreated)
                {
                    return Ok(result_message);
                }
                else
                {
                    return BadRequest(result_message);
                }
            }
			catch (Exception ex)
			{
				Console.WriteLine("error in implant creation : " + ex.Message);
				return BadRequest(ex.Message);
			}

        }

	}
}
