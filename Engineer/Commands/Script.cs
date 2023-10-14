//using DynamicEngLoading;
//using Engineer.Modules;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Engineer.Commands
//{
//    public class Script : EngineerCommand
//    {
//        public override string Name => "script";

//        public override async Task Execute(EngineerTask task)
//        {
//            try
//            {
//                if (task.Arguments.TryGetValue("/method", out string method))
//                {
//                    method = method.ToLower().Trim();
//                    //deserialize the internal task from the task.file property
//                    if (!task.Arguments.TryGetValue("/script", out var script))
//                    {
//                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: no file sent, use the /script argument with a *.cs file as the value for example /script whoami.cs", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
//                    }
//                    if (method.Equals("load"))
//                    {
//                        await ScriptModule.LoadCommandFromScript(script, task);
//                    }
//                    else if (method.Equals("execute"))
//                    {
//                        EngineerTask InternalTasking = task.File.JsonDeserialize<EngineerTask>();
//                        await ScriptModule.ExecuteScript(script, InternalTasking, task);
//                    }
//                }
//                else
//                {
//                    ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("no /method argument given, values are load or execute", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//                Console.WriteLine(ex.StackTrace);
//                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"{ex.Message}", task, EngTaskStatus.Failed, TaskResponseType.String);
//            }
            
//        }
//    }
//}
