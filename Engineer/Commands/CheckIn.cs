using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class CheckIn : EngineerCommand
    {
        public override string Name => "checkIn";

        public override async Task Execute(EngineerTask task)
        {
            //Console.WriteLine("Checking In");
            task.Arguments.TryGetValue("/parentid", out string parentId);
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(Program._metadata.Id+"\n" + parentId,task,EngTaskStatus.Complete,TaskResponseType.String); //gives back the engineers metadata and the parent so the teamserver can make the path
        }
    }
}
