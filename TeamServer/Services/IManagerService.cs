using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;
/* allows for instructions to be called, holds no objects, or properties/ values
 * linked to controllers with dependencies injection so things can be called in other areas of the code easily. 
 * you want one service per controller 
 */

namespace TeamServer.Services
{
    public interface ImanagerService
	{
		void Addmanager(manager manager);
		IEnumerable<manager> Getmanagers();

		manager Getmanager(string name);
		void Removemanager(manager manager);
	}

	public class managerService : ImanagerService
	{
		public static readonly List<manager> _managers = new();

		public void Addmanager(manager manager)
		{
			_managers.Add(manager);
		}

		public IEnumerable<manager> Getmanagers()
		{
			return _managers;
		}

		public manager Getmanager(string name)
		{
			return Getmanagers().FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase)); // labmda takes input, assigns it too thing left of => and the expression on the right uses it for something 
		}

		public void Removemanager(manager manager)
		{
			_managers.Remove(manager);
		}
		
		public static async Task StartManagersFromDB(List<Httpmanager> _httpmanagers)
		{
			var _EngineerService = new EngineerService();
			foreach (Httpmanager _manager in _httpmanagers)
			{
				Console.WriteLine($"Calling Init on {_manager.Name}");
				_manager.Init(_EngineerService);
				_manager.Start();
				Console.WriteLine($"{_manager.Name} started should be listening on {_manager.BindAddress}:{_manager.BindPort}");
			}
		}
	}
}
