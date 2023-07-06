using System;
using System.IO;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Cat : EngineerCommand
    {
        public override string Name => "cat";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                if (task.Arguments.TryGetValue("/file", out string file))
                {
                    //read the content of file and return it
                    string content = File.ReadAllText(file);
                    if (content.Length == 0)
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("file does not exist or has no content", task, EngTaskStatus.CompleteWithErrors,TaskResponseType.String);
                        return;
                    }

                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(content,task,EngTaskStatus.Complete,TaskResponseType.String);
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("no /file argument given",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
