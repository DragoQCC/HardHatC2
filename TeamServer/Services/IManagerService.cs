using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models;

/* allows for instructions to be called, holds no objects, or properties/ values
 * linked to controllers with dependencies injection so things can be called in other areas of the code easily.
 * you want one service per controller
 */

namespace HardHatCore.TeamServer.Services
{
    public interface ImanagerService
	{
        protected static readonly List<Manager> _managers = new();
        public static void Addmanager(Manager manager)
		{
			_managers.Add(manager);
		}

		public static void AddManagers(IEnumerable<Manager> managersList)
		{
			_managers.AddRange(managersList);
		}

		public static IEnumerable<Manager> Getmanagers()
		{             
			return _managers;
		}

		public static Manager Getmanager(string name)
		{
            return Getmanagers().FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        public static void Removemanager(Manager manager)
        {
            _managers.Remove(manager);
        }
    }

	public class managerService : ImanagerService
	{
        public void Addmanager(Manager manager)
		{
            ImanagerService.Addmanager(manager);
        }

		public IEnumerable<Manager> Getmanagers()
		{
			return ImanagerService.Getmanagers();
		}

		public Manager Getmanager(string name)
		{
			return ImanagerService.Getmanager(name);
		}

		public void Removemanager(Manager manager)
		{
            ImanagerService.Removemanager(manager);
		}
		
		public static async Task StartManagersFromDB(List<HttpManager> _httpmanagers)
		{
			var _EngineerService = new EngineerService();
			foreach (HttpManager _manager in _httpmanagers)
			{
				Console.WriteLine($"Calling start on {_manager.Name}");
				_manager.Start();
				Console.WriteLine($"{_manager.Name} started should be listening on {_manager.BindAddress}:{_manager.BindPort}");
			}
		}
	}
}
