using System.Threading.Tasks;
using System.Threading;
using System;
using FastEndpoints;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class DeleteImplantTask : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Delete("Implants/{implantId}/tasks/{taskId}");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            string ImplantId = Route<string>("implantId");
            string taskId = Route<string>("taskId");

            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var Implant = _ImplantSvc.GetExtImplant(ImplantId);
            if (Implant is null)
            {
                ThrowError($"implant not found, {ImplantId} may not a valid Id");
            }

            var task = await Implant.GetTask(taskId);
            if (task is null)
            {
                ThrowError("task not found to delete");
            }
            if (await Implant.RemoveTask(task))
            {
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"task {task.Command} {task.Arguments} deleted", Status = "info" });
                LoggingService.TaskLogger.ForContext("Task", task, true).ForContext("Implant_Id", ImplantId).Information($"task {task.Command} {task.Arguments} deleted");
                HardHatHub.NotifyTaskDeletion(Implant.Metadata.Id, task.Id);
                //connect to db and delete task
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }
                //check if it is also in the task result dao table and delete it from there as well 
                var taskResult = DatabaseService.AsyncConnection.Table<ExtImplantTaskResult_DAO>().Where(t => t.TaskId == task.Id).FirstOrDefaultAsync();
                if (taskResult != null)
                {
                    DatabaseService.AsyncConnection.DeleteAsync(taskResult);
                }
                SendOkAsync();
            }
            SendNotFoundAsync();
        }
    }
}
