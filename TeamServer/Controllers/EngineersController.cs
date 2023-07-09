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
using ApiModels.Shared;
//using DynamicEngLoading;

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
			if (request.TaskingExtras.TryGetValue("PostExImplantRequest", out var requestObject))
			{
				SpawnEngineerRequest implantSpwnRequest = requestObject.Deserialize<SpawnEngineerRequest>();
				if (request.Arguments.TryGetValue("/method", out string method))
				{
					method = method.Trim();
					if (method.Equals("psexec", StringComparison.CurrentCultureIgnoreCase))
					{
						implantSpwnRequest.complieType = EngCompileType.serviceexe;
					}
					else
					{
						implantSpwnRequest.complieType = EngCompileType.exe;
					}
				}
				else
				{
					implantSpwnRequest.complieType = EngCompileType.exe;

				}
				bool isCreated = EngineerService.CreateEngineers(implantSpwnRequest, out string result_message);
				if (!isCreated)
				{
					Console.WriteLine("error in post ex command implant creation : " + result_message);
					return BadRequest(result_message);
				}
			}
			var engineer = _Engineers.GetEngineer(engineerId);
			if (engineer is null) return NotFound();

			var task = new EngineerTask(request.taskID, request.Command, request.Arguments, request.File, request.IsBlocking);

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
				LoggingService.TaskLogger.ForContext("Task", LoggingTask, true).ForContext("Engineer_Id", engineerId).Information($"task {taskHeader.Command} queued for execution");
				return Created(path, task);
			}
			return NotFound();
		}
        
		[HttpPost(Name = "CreateEngineer")]
		public IActionResult CreateEngineer([FromBody] SpawnEngineerRequest request)
		{
           var created_ok = EngineerService.CreateEngineers(request,out string resultMessage);
			if (created_ok)
			{
				return Ok(resultMessage);
			}
			else
			{
                return BadRequest(resultMessage);
            }
        }

	}
}
