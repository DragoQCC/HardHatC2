using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DynamicEngLoading;


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
                var arpProcess = new Process();
                arpProcess.StartInfo.FileName = "arp.exe";
                arpProcess.StartInfo.Arguments = "-a";
                arpProcess.StartInfo.UseShellExecute = false;
                arpProcess.StartInfo.RedirectStandardOutput = true;
                arpProcess.Start();
                var output = arpProcess.StandardOutput.ReadToEnd();
                arpProcess.WaitForExit();
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output, task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(e.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
