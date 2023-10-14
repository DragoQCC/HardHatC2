//using DynamicEngLoading;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using CSScriptLibrary;
//using System.Runtime.Hosting;
//using System.Reflection;

//namespace Engineer.Modules
//{
//    [Module(Name: "ScriptModule")]
//    public class ScriptModule
//    {
//        public static async Task ExecuteScript(string script, EngineerTask internalTask, EngineerTask scriptExecTask)
//        {
//            try
//            {

//                EngineerCommand result = (EngineerCommand)CSScript.Evaluator.LoadCode(script);
//                await result.Execute(internalTask);
//                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Script executed successfully", scriptExecTask, EngTaskStatus.Complete, TaskResponseType.String);
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.Message);
//                Console.WriteLine(e.StackTrace);
//                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"{e.Message}", scriptExecTask, EngTaskStatus.Failed, TaskResponseType.String);
//            }
//        }

//        public static async Task LoadCommandFromScript(string script, EngineerTask task)
//        {
//            try
//            {
//                EngineerCommand result = (EngineerCommand)CSScript.Evaluator.LoadCode(script);
//                //use reflection to add the command to the list of commands in Program._Commands
//                IEnumerable<IEngineerCommand> storeCommands = (IEnumerable<IEngineerCommand>)Assembly.GetCallingAssembly().GetType("Engineer.Program").GetField("_commands", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
//                //update the list of commands
//                List<IEngineerCommand> newCommandsTemp = storeCommands.ToList();
//                newCommandsTemp.Add(result);
//                Assembly.GetCallingAssembly().GetType("Engineer.Program").GetField("_commands", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, newCommandsTemp);
//                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"New Command {result.Name} added successfully", task, EngTaskStatus.Complete, TaskResponseType.String);

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
