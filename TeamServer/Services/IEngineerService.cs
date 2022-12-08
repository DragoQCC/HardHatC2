using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Models;

namespace TeamServer.Services
{
	public interface IEngineerService
	{
		void AddEngineer(Engineer Engineer);
		IEnumerable<Engineer> GetEngineers();
		Engineer GetEngineer(string id);
		void RemoveEngineer(Engineer Engineer);
	}

	public class EngineerService : IEngineerService
	{
		public readonly List<Engineer> _engineers = new(); //readonly works here because a list is ref type, works even if the data type i nthe list if value type like a list of ints.
															// so I also cant make a new list and try and assin it to this one I can only mess with this list instance not replace it.

		public void AddEngineer(Engineer Engineer)
		{
			_engineers.Add(Engineer);
		}
		public IEnumerable<Engineer> GetEngineers()
		{
			return _engineers;
		}
		public Engineer GetEngineer(string id)
		{
			return GetEngineers().FirstOrDefault(a => a.engineerMetadata.Id.Equals(id));
		}
		public void RemoveEngineer(Engineer Engineer)
		{
			_engineers.Remove(Engineer);
		}
	}
}
