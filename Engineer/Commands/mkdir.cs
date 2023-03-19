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
    internal class mkdir : EngineerCommand
    {
        public override string Name => "mkdir";

        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/path", out string path);
            if (Directory.Exists(path))
            {
                Tasking.FillTaskResults("error: " + "Directory already exists",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                return;
            }
            //try to create directory 
            try
            {
                Directory.CreateDirectory(path);
                Tasking.FillTaskResults("Directory created",task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception e)
            {
                Tasking.FillTaskResults("error: " + e.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
