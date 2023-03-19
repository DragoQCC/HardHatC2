using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
	public class TestCommand : EngineerCommand
	{
		public override string Name => "TestCommand";

		public override async Task Execute(EngineerTask task)
		{
			Tasking.FillTaskResults("Hello from test command", task, EngTaskStatus.Complete,TaskResponseType.String);
		}

	}
}
