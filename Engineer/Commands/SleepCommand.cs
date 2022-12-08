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

        public override string Execute(EngineerTask task)
        {
            if (task.Arguments != null)
            {
                task.Arguments.TryGetValue("/time", out var sleep);

                EngCommBase.Sleep = int.Parse(sleep)*1000;
                return "Sleep set to " + EngCommBase.Sleep/1000;
            }
            else
                return "error: " + "Sleep setting change failed, please provide a number in seconds like so Sleep 5";
            
        }
    }
}
