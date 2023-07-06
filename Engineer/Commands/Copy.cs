using System;
using System.IO;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Copy : EngineerCommand
    {
        public override string Name => "copy";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                if (!task.Arguments.TryGetValue("/file", out string file))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "no file to copy set pls use the /file key", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                if (!task.Arguments.TryGetValue("/dest", out string destination))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "no destination file set pls use the /destination key", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                //copy file to destionation
                File.Copy(file, destination);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Copied {file} to {destination}", task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
