using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Engineer.Models;
using Engineer.Functions;

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
                    Tasking.FillTaskResults("error: " + "no file to copy set pls use the /file key", task, EngTaskStatus.FailedWithWarnings);
                    return;
                }
                if (!task.Arguments.TryGetValue("/dest", out string destination))
                {
                    Tasking.FillTaskResults("error: " + "no destination file set pls use the /destination key", task, EngTaskStatus.FailedWithWarnings);
                    return;
                }
                //copy file to destionation
                File.Copy(file, destination);
                Tasking.FillTaskResults($"Copied {file} to {destination}", task, EngTaskStatus.Complete);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed);
            }

        }
    }
}
