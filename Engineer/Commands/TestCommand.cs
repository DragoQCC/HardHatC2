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

		public override string Execute(EngineerTask task)
		{
			return "Hello from test command";
		}

	}
}
