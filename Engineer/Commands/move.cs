using System;
using System.IO;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class move : EngineerCommand
    {
        public override string Name => "move";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                task.Arguments.TryGetValue("/dest", out string destination);
                task.Arguments.TryGetValue("/file", out string file);

                //move file to destination location
                if (File.Exists(file))
                {
                    File.Move(file, destination);
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("File moved", task, EngTaskStatus.Complete,TaskResponseType.String);
                    return;
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "file not found", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            catch (Exception e)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + e.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
