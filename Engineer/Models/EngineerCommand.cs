using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Models
{
	public abstract class EngineerCommand
	{
		public abstract string Name { get; }


		public abstract string Execute(EngineerTask task);
	}
}
