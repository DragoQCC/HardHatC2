using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
	public class TestCommand : EngineerCommand
	{
		public override string Name => "TestCommand";

		public override async Task Execute(EngineerTask task)
		{
			ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Hello from test command", task, EngTaskStatus.Complete,TaskResponseType.String);
		}

	}
}
