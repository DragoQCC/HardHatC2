using System;
using Microsoft.AspNetCore.Mvc;
using TeamServer.Models;
using TeamServer.Services;
using ApiModels.Requests;
using System.Threading.Tasks;
using TeamServer.Models.Extras;
using TeamServer.Models.Managers;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using TeamServer.Models.Dbstorage;
using System.Collections.Generic;

/*
 A controller is responsible for controlling the way that a user interacts with an MVC application.
 A controller contains the flow control logic & determines what response to send back to a user when a user makes a browser request
It then uses Models & services as needed to run those checks, grab data, store new data, etc. 
 */
namespace TeamServer.Controllers
{
    [Authorize(Roles ="Operator,TeamLead")]
    [ApiController]         
	[Route("[Controller]")]									// Route used for the url we go too interact with API like "curl http:localhost:5000/managers/managersname" it auto drops controller keyword and just uses stuff before suffix
	public class managersController : ControllerBase
	{
		public readonly ImanagerService _managers;
		public readonly  IEngineerService _EngineerService;

        public managersController(ImanagerService managers, IEngineerService EngineerService)  //constructor for class
        {
            _managers = managers;
            _EngineerService = EngineerService;
        }

        
		[HttpGet(Name = "RetrieveAllmanagers")]												// used when API controller wants to mark type of interaction that can hapen 
		public IActionResult Getmanagers()						// Get request for all managers
		{
			var managers = _managers.Getmanagers();
			return Ok(managers);
		}

		[HttpGet("{name}", Name = "Retrievemanager")]
		public IActionResult Getmanager(string name)		// get request based off of name of manager
		{
			var manager = _managers.Getmanager(name);
			if (manager is null)
			{
				return NotFound();							// no name or name not found returns a 404
			}
			return Ok(manager);							// found returns an ok on the manager
		}

		[HttpPost(Name ="Startmanager")]
		public IActionResult Startmanager([FromBody] StartManagerRequest request) //post request to take in Name and Port to set from body lets the controller take out the info sent in the bos of the psot so we get the name and port 
		{
			if (request.managertype == StartManagerRequest.ManagerType.http || request.managertype == StartManagerRequest.ManagerType.https)
			{
                var manager = new Httpmanager(request.Name, request.ConnectionAddress,request.ConnectionPort,request.BindAddress,request.BindPort, request.IsSecure, request.C2profile);

				manager.Init(_EngineerService);
				manager.Start();
				_managers.Addmanager(manager);
				if(DatabaseService.AsyncConnection == null)
				{
                    DatabaseService.ConnectDb();
                }
				DatabaseService.AsyncConnection.InsertAsync((HttpManager_DAO)manager);

				var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
				var path = $"{root}/{manager.Name}";
				HardHatHub.UpdateManagerList(manager);
				HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"{manager.Type} manager {manager.Name} created on {manager.ConnectionAddress}:{manager.ConnectionPort}", Status = "success" });
                LoggingService.EventLogger.Information($"{manager.Type} manager created.{@manager}", manager);
                return Created(path, manager);
			}
			else if(request.managertype == StartManagerRequest.ManagerType.tcp)
			{
                TCPManager manager = null;
                if (request.connectionMode == StartManagerRequest.ConnectionMode.bind)
				{
                    manager = new TCPManager(request.Name,request.ListenPort, request.IsLocalHost);
                }
                else if(request.connectionMode == StartManagerRequest.ConnectionMode.reverse)
				{
                    manager = new TCPManager(request.Name, request.ConnectionAddress, request.BindPort);
                }
                manager.Init(_EngineerService);
                manager.Start();
                _managers.Addmanager(manager);
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }
                DatabaseService.AsyncConnection.InsertAsync((TCPManager_DAO)manager);
                var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
                var path = $"{root}/{manager.Name}";
                HardHatHub.UpdateManagerList(manager);
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"{manager.connectionMode} TCP manager {manager.Name}, created", Status = "success" });
                LoggingService.EventLogger.Information("TCP manager created.{@manager}", manager);
                return Created(path, manager);

            }
            else
            {
                SMBmanager manager = null;
                if (request.connectionMode == StartManagerRequest.ConnectionMode.bind)
				{
					manager = new SMBmanager(request.Name, request.NamedPipe);
				}
				else if(request.connectionMode == StartManagerRequest.ConnectionMode.reverse)
				{
                    manager = new SMBmanager(request.Name, request.NamedPipe, request.ConnectionAddress);
                }
                manager.Init(_EngineerService);
                manager.Start();
                _managers.Addmanager(manager);
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }
                DatabaseService.AsyncConnection.InsertAsync((SMBManager_DAO)manager);
                var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
                var path = $"{root}/{manager.Name}";
                HardHatHub.UpdateManagerList(manager);
				HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"SMB manager {manager.Name}, with pipe name {manager.NamedPipe}", Status = "success" });
                LoggingService.EventLogger.Information("SMB manager created.{@manager}", manager);
                return Created(path, manager);
            }
        }


        [AllowAnonymous]
        [HttpPost("addDB",Name="AddManagersFromDB")]
		public IActionResult AddManagersFromDB([FromBody] List<Httpmanager> _managers)
		{
            foreach (Httpmanager _manager in _managers)
            {
	            Console.WriteLine($"Calling Init on {_manager.Name}");
				_manager.Init(_EngineerService);
	            _manager.Start();
				Console.WriteLine($"{_manager.Name} started should be listening on {_manager.ConnectionAddress}:{_manager.ConnectionPort}");
				return Ok();
            }
            return BadRequest();
        }
		

        [HttpDelete("{name}",Name = "Deletemanager")]
		public IActionResult Stopmanager(string name)
		{
			var manager = _managers.Getmanager(name);
			if (manager is null)
			{
				return NotFound();
			}

			manager.Stop();
			_managers.Removemanager(manager);
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.DeleteAsync((HttpManager_DAO)manager);
            HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Manager {manager.Name} removed", Status = "warning" });
            LoggingService.EventLogger.Warning("Manager {manager.Name} removed", manager.Name);
            return NoContent();
		}

        
        

    }
}
