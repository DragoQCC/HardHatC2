using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class SpawnTo : EngineerCommand
    {
        public override string Name => "spawnto";
       // public static string spawnToPath = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorsvw.exe";
        public static string spawnToPath = @"C:\Windows\System32\notepad.exe";
        public override async Task Execute(EngineerTask task)
        {
             if (task.Arguments.TryGetValue("/path", out string path))
                {
                spawnToPath = path.TrimStart(' ');
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("SpawnTo Path Set as " + spawnToPath, task, EngTaskStatus.Complete,TaskResponseType.String);
                return;
                }
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " + "SpawnTo Path Not Set using default", task, EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
        }
    }
}
