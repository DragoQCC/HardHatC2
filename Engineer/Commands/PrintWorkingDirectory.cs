using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
	public class PrintWorkingDirectory : EngineerCommand
	{
		public override string Name => "pwd" ;
	
	public override string Execute(EngineerTask task)
		{
			return Directory.GetCurrentDirectory();  // from System.IO
		}
	}
}
