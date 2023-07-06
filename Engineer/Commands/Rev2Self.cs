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
                if (h_DynInv_Methods.AdvApi32FuncWrapper.RevertToSelf())
                {
                    h_DynInv_Methods.AdvApi32FuncWrapper.OpenProcessToken(Process.GetCurrentProcess().Handle, h_DynInv.Win32.Advapi32.TOKEN_ALL_ACCESS, out var htest);
                    string result = $" process handle is: {htest}\n" + "Dropped impersonation, reverted to previous user";

                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(result,task,EngTaskStatus.Complete,TaskResponseType.String);
                    return;
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Failed to drop token", task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
