using System;
using System.Linq;
using FastEndpoints;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Services;
using System.Threading.Tasks;
using System.Threading;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_Management;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class DeleteImplantTasks : Endpoint<string>
    {
        public override void Configure()
        {
            Delete("Implants/{implantId}");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(string confirmMessage,CancellationToken ct)
        {
            if (!confirmMessage.Equals("Delete all results from this implant", StringComparison.CurrentCultureIgnoreCase))
            {
                ThrowError("Please confirm you want to delete all tasks by sending the string 'Delete all results from this implant' in the body of the request");
            }
            string ImplantId = Route<string>("implantId");
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var Implant = _ImplantSvc.GetExtImplant(ImplantId);
            if (Implant is null)
            {
                ThrowError($"implant not found, double check {ImplantId} is a valid implant Id");
            }

            var taskList = Implant.GetTasks().Result;

            if (taskList.Any())
            {
                foreach (var task in taskList)
                {
                    HardHatHub.NotifyTaskDeletion(Implant.Metadata.Id, task.Id);
                    //connect to db and delete task
                    if (DatabaseService.AsyncConnection == null)
                    {
                        DatabaseService.ConnectDb();
                    }
                    DatabaseService.AsyncConnection.DeleteAsync((ExtImplantTask_DAO)task);
                    //check if it is also in the task result dao table and delete it from there as well 
                    var taskResult = DatabaseService.AsyncConnection.Table<ExtImplantTaskResult_DAO>().Where(t => t.TaskId == task.Id).FirstOrDefaultAsync();
                    if (taskResult != null)
                    {
                        DatabaseService.AsyncConnection.DeleteAsync(taskResult);
                    }
                }
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"all tasks deleted", Status = "info" });
                LoggingService.TaskLogger.ForContext("Implant_Id", ImplantId).Information($"all tasks deleted");
                SendOkAsync();
            }
            SendNotFoundAsync();
        }
    }
    }
