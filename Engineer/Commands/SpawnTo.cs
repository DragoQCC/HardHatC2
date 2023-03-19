using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class SpawnTo : EngineerCommand
    {
        public override string Name => "spawnto";
       // public static string spawnToPath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorsvw.exe";
        public static string spawnToPath = @"C:\Windows\System32\calc.exe";
        public override async Task Execute(EngineerTask task)
        {
             if (task.Arguments.TryGetValue("/path", out string path))
                {
                spawnToPath = path.TrimStart(' ');
                Tasking.FillTaskResults("SpawnTo Path Set as " + spawnToPath, task, EngTaskStatus.Complete,TaskResponseType.String);
                return;
                }
            Tasking.FillTaskResults("error: " + "SpawnTo Path Not Set using default", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
        }
    }
}
