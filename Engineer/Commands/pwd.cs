using System.IO;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
	public class pwd : EngineerCommand
	{
		public override string Name => "pwd" ;
	
	public override async Task Execute(EngineerTask task)
		{
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(Directory.GetCurrentDirectory(),task,EngTaskStatus.Complete,TaskResponseType.String); 
		}
	}
}
