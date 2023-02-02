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
    internal class Delete : EngineerCommand
    {
        public override string Name => "delete";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                if (!task.Arguments.TryGetValue("/file", out string file))
                {
                    Tasking.FillTaskResults("error: " + "no file to delete set pls use the /file key", task, EngTaskStatus.FailedWithWarnings);
                    return;
                }
                //delete file
                File.Delete(file);
                Tasking.FillTaskResults($"Deleted {file}", task, EngTaskStatus.Complete);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed);
            }
        }
    }
}
