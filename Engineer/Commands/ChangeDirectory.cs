using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
	public class ChangeDirectory : EngineerCommand
	{
		public override string Name => "cd" ;
	
	public override string Execute(EngineerTask task)
		{
			try
			{
				if (!task.Arguments.TryGetValue("/path", out string path)) // if it fails to get a value from this argument then it will use the current path
				{
					path = Directory.GetCurrentDirectory();
				}
				Directory.SetCurrentDirectory(path);
				return Directory.GetCurrentDirectory(); // needs a return since string should print updated dir.
			}
			catch (Exception ex)
			{
				return "error: " + ex.Message;
			}
		}
	}
}
