using Microsoft.AspNetCore.Mvc;
using TeamServer.Models;
using TeamServer.Services;
using ApiModels.Requests;
using System.Threading.Tasks;
using TeamServer.Models.Extras;
using TeamServer.Models.Managers;

/*
 A controller is responsible for controlling the way that a user interacts with an MVC application.
 A controller contains the flow control logic & determines what response to send back to a user when a user makes a browser request
It then uses Models & services as needed to run those checks, grab data, store new data, etc. 
 */
namespace TeamServer.Controllers
{
	[ApiController]         
	[Route("[Controller]")]									// Route used for the url we go too interact with API like "curl http:localhost:5000/managers/managersname" it auto drops controller keyword and just uses stuff before suffix
	public class managersController : ControllerBase
	{
		public readonly ImanagerService _managers;
		private readonly IEngineerService _EngineerService;

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
                var manager = new Httpmanager(request.Name, request.ConnectionPort, request.ConnectionAddress, request.IsSecure, request.C2profile);

				manager.Init(_EngineerService);
				manager.Start();
				_managers.Addmanager(manager);

				var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
				var path = $"{root}/{manager.Name}";
				HardHatHub.UpdateManagerList(manager);
				HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"HTTP/HTTPS manager {manager.Name} created on {manager.ConnectionAddress}:{manager.ConnectionPort}", Status = "success" });
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
                var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
                var path = $"{root}/{manager.Name}";
                HardHatHub.UpdateManagerList(manager);
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"{manager.connectionMode} TCP manager {manager.Name}, created", Status = "success" });
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
                var root = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
                var path = $"{root}/{manager.Name}";
                HardHatHub.UpdateManagerList(manager);
				HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"SMB manager {manager.Name}, with pipe name {manager.NamedPipe}", Status = "success" });
				return Created(path, manager);
            }
        }

		//http put to allow users to update a maangers values 
		//[HttpPut("{name}", Name = "UpdateManager")]
		//public IActionResult UpdateManager(string name, [FromBody] StartManagerRequest request)
		//{
  //          var manager = _managers.Getmanager(name);
  //          if (manager is null)
  //          {
  //              return NotFound();
  //          }
           


  //      }

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
			HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"Manager {manager.Name} removed", Status = "warning" });
			return NoContent();
		}

        
        

    }
}
