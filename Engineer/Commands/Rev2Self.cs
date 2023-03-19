using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using Engineer.Functions;
using Engineer.Models;

namespace Engineer.Commands
{
    internal class Rev2Self : EngineerCommand
    {
        public override string Name => "rev2self";

        public override async Task Execute(EngineerTask task)
        {
            {
                if (WinAPIs.Advapi.RevertToSelf())
                {
                    WinAPIs.Advapi.OpenProcessToken(Process.GetCurrentProcess().Handle, WinAPIs.Advapi.TOKEN_ALL_ACCESS, out var htest);
                    string result = $" process handle is: {htest}\n" + "Dropped impersonation, reverted to previous user";

                    Tasking.FillTaskResults(result,task,EngTaskStatus.Complete,TaskResponseType.String);
                    return;
                }
                Tasking.FillTaskResults("error: " + "Failed to drop token", task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
