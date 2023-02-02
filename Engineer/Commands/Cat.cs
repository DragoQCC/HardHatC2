using Engineer.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engineer.Functions;

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
                        Tasking.FillTaskResults("file does not exist or has no content", task, EngTaskStatus.CompleteWithErrors);
                        return;
                    }

                    Tasking.FillTaskResults(content,task,EngTaskStatus.Complete);
                }
                Tasking.FillTaskResults("no /file argument given",task,EngTaskStatus.FailedWithWarnings);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults("error: " + ex.Message, task, EngTaskStatus.Failed);
            }
        }
    }
}
