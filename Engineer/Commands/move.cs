using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Engineer.Functions;

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
                    Tasking.FillTaskResults("File moved", task, EngTaskStatus.Complete,TaskResponseType.String);
                    return;
                }
                Tasking.FillTaskResults("error: " + "file not found", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            catch (Exception e)
            {
                Tasking.FillTaskResults("error: " + e.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
