using FastEndpoints;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Management;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class DeleteAllTasks : Endpoint<string>
    {
        public override void Configure()
        {
            Delete("Implants/");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(string confirmMessage, CancellationToken ct)
        {
            if (!confirmMessage.Equals("Delete all results from all implants", StringComparison.CurrentCultureIgnoreCase))
            {
                ThrowError("Please confirm you want to delete all tasks by sending the string 'Delete all results from all implants' in the body of the request");
            }
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            List<ExtImplant_Base> ImplantList = _ImplantSvc.GetExtImplants().ToList();

            if (ImplantList.Count > 0)
            {
                foreach (var Implant in ImplantList)
                {
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
                    }
                }
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"all tasks deleted", Status = "info" });
                LoggingService.TaskLogger.Information($"all tasks deleted");
                SendOkAsync("Tasks deleted");
            }
            SendNotFoundAsync();
        }
    }
}
