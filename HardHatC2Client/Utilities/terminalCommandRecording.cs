using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;

namespace HardHatCore.HardHatC2Client.Utilities
{
    public class terminalCommandRecording
    {
        public static Dictionary<string,ConcurrentQueue<string>> TerminalOutput = new();
        public static Dictionary<string, ConcurrentQueue<string>> TerminalInput = new();
        public static async Task TerminalCommandExecute(string terminalId)
        {
            if(!TerminalOutput.ContainsKey(terminalId))
            {
                TerminalOutput.Add(terminalId, new ConcurrentQueue<string>());
            }
            if (!TerminalInput.ContainsKey(terminalId))
            {
                TerminalInput.Add(terminalId, new ConcurrentQueue<string>());
            }


            //check the current operating system and make sure its linux
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Console.WriteLine("Linux OS detected starting terminal");
                //create a new process
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = false
                    }
                };
                //add the output to the string builder
                process.OutputDataReceived += new DataReceivedEventHandler(((sendingobj, e) => CmdOutputDataHandler(sendingobj, e, terminalId)));
                process.ErrorDataReceived += new DataReceivedEventHandler(((sendingobj, e) => CmdOutputDataHandler(sendingobj, e, terminalId)));
                StringBuilder strInput = new StringBuilder();
                //start the process
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (true)
                {
                    try
                    {
                        //sleep until the queue is not empty
                        while (TerminalInput[terminalId].IsEmpty)
                        {
                            Thread.Sleep(100);
                        }
                        // read reader and if null wait for a new message
                        string message = TerminalInput[terminalId].TryDequeue(out message) ? message : "";
                        if(message.Equals("Exit terminal",StringComparison.CurrentCultureIgnoreCase))
                        {
                            process.Kill();
                            break;
                        }
                        // reads the input and adds it to the stringbuilder, then its sent to the program as input
                        strInput.Append(message);
                        process.StandardInput.WriteLine(strInput);
                        strInput.Remove(0, strInput.Length);
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }
                    Thread.Sleep(10);
                }

            }
            else if((System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)))
            {
                Console.WriteLine("windows OS detected starting powershell terminal");
                //create a new process
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = "-nologo",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = false
                    }
                };

                //add the output to the string builder
                process.OutputDataReceived += new DataReceivedEventHandler(((sendingobj,e)=>CmdOutputDataHandler(sendingobj, e, terminalId)));
                process.ErrorDataReceived +=  new DataReceivedEventHandler(((sendingobj,e)=>CmdOutputDataHandler(sendingobj, e, terminalId)));
                StringBuilder strInput = new StringBuilder();
                //start the process
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (true)
                {
                    try
                    {
                        //sleep until the queue is not empty
                        while (TerminalInput[terminalId].IsEmpty)
                        {
                            Thread.Sleep(100);
                        }
                        // read reader and if null wait for a new message
                        string message = TerminalInput[terminalId].TryDequeue(out message) ? message : "";
                        if (message.Equals("Exit terminal", StringComparison.CurrentCultureIgnoreCase))
                        {
                            process.Kill();
                            break;
                        }
                        // reads the input and adds it to the stringbuilder, then its sent to the program as input
                        strInput.Append(message);
                        process.StandardInput.WriteLine(strInput);
                        strInput.Remove(0, strInput.Length);
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }
                    Thread.Sleep(10);
                }

            }
            else
            {
                Console.WriteLine("client OS not supported");
            }
        }
        public static void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine, string terminalId)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    TerminalOutput[terminalId].Enqueue(strOutput.ToString());
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
            }
        }
    }
}
