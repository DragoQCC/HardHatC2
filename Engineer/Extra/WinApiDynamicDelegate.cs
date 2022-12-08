using Engineer.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Extra
{
    public class WinApiDynamicDelegate
    {
        //generic delegate
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate string GenericDelegate(string input);
        //generic delegate
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate string GenericArrayDelegate(string[] input);
        //sleep dll export delegate
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int SleepDelegate(SleepEncrypt.PARAMS pARAMS);
        // nt create section ntdll
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate uint NtCreateSection(ref IntPtr SectionHandle, uint DesiredAccess, IntPtr ObjectAttributes, ref ulong MaximumSize, uint SectionPageProtection, uint AllocationAttributes, IntPtr FileHandle);
        // nt map view of section ntdll
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate uint NtMapViewOfSection(IntPtr SectionHandle, IntPtr ProcessHandle, out IntPtr BaseAddress, IntPtr ZeroBits, IntPtr CommitSize, IntPtr SectionOffset, out ulong ViewSize, uint InheritDisposition, uint AllocationType, uint Win32Protect);
        // create process w kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool CreateProcessW(string lpApplicationName, string lpCommandLine, ref WinAPIs.Kernel32.SECURITY_ATTRIBUTES lpProcessAttributes, ref WinAPIs.Kernel32.SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref WinAPIs.Kernel32.STARTUPINFOEX lpStartupInfo, out WinAPIs.Kernel32.PROCESS_INFORMATION lpProcessInformation);
        //write process mem kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);
        // NtProtectVirtualMemory
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate h_reprobate.NTSTATUS NtProtectVirtualMemory(IntPtr ProcessHandle, ref IntPtr BaseAddress, ref IntPtr RegionSize, uint NewAccessProtection, ref uint OldAccessProtection);
        //VirtualProtect kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool VirtualProtect(IntPtr lpAddress, uint dwSize, int flNewProtect, out int lpflOldProtect);
        //Queue User APC kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, uint dwData);
        //resume thread kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint ResumeThread(IntPtr hThread);
        //create thread kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr param, uint dwCreationFlags, ref uint lpThreadId);
        //virtual alloc kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr VirtualAlloc(IntPtr lpStartAddr, uint size, uint flAllocationType, uint flProtect);
        //virtual alloc Ex kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        //wait for single object kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        //can we do Assembly Load(byte[])
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate byte[] Load(byte[] rawAssembly);
        //Load Library kernel32 
        [UnmanagedFunctionPointer(CallingConvention.StdCall,CharSet =CharSet.Unicode)]
        public delegate IntPtr LoadLibraryW(string library);
        //get proc address kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetProcAddress(IntPtr libPtr, string function);
        //free library kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool freeLibrary(IntPtr library);
        // convert thread to fiber kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool ConvertThreadToFiber(IntPtr lpParameter);
        // createFiber kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr CreateFiber(uint dwStackSize, uint lpStartAddress, IntPtr lpParameter);
        //switch to fiber kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint SwitchToFiber(IntPtr lpFiber);
        //get current thread kernel32
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetCurrentThread();

    }
}
