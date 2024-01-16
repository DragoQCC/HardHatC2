using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using FastEndpoints;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using FastEndpoints.Security;

namespace HardHatCore.TeamServer.Endpoints.ImplantManageEndpoints
{
    public class TaskImplant : Endpoint<TaskExtImplantRequest_Base,ExtImplantTask_Base>
    {
        public override void Configure()
        {
            Post("/Implants/{implantId}");
            Roles(new string[] { "Operator", "TeamLead" });
            Options(x => x.WithTags("Implants"));
        }

        public override async Task HandleAsync(TaskExtImplantRequest_Base request, CancellationToken ct)
        {
            string implantId = Route<string>("implantId");
            if (request.TaskingExtras.TryGetValue("PostExImplantRequest", out var requestObject))
            {
                ExtImplantCreateRequest_Base implantSpwnRequest = requestObject.Deserialize<ExtImplantCreateRequest_Base>();
                if (request.Arguments.TryGetValue("/method", out string method))
                {
                    method = method.Trim();
                    if (method.Equals("psexec", StringComparison.CurrentCultureIgnoreCase))
                    {
                        implantSpwnRequest.complieType = ImpCompileType.serviceexe;
                    }
                    else
                    {
                        implantSpwnRequest.complieType = ImpCompileType.exe;
                    }
                }
                else
                {
                    implantSpwnRequest.complieType = ImpCompileType.exe;

                }
                bool isCreated = false;
                string result_message = null;
                var svc_plugins = Plugin_Management.PluginService.pluginHub.implant_servicePlugins;
                var svc_plugin = svc_plugins.GetPluginEnumerableResult(implantSpwnRequest.implantType);

                isCreated = svc_plugin.CreateExtImplant(implantSpwnRequest, out result_message);
                if (!isCreated)
                {
                    Console.WriteLine("error in post ex command implant creation : " + result_message);
                    ThrowError(result_message);
                }
            }
            var _ImplantSvc = PluginService.GetImpServicePlugin("Default");
            var implant = _ImplantSvc.GetExtImplant(implantId);
            if (implant is null)
            {
                ThrowError("Failed to find implant");
            }

            //read the user from the jwt token
            HttpContext.Request.Headers.TryGetValue("Authorization", out var bearerTokenHeader);
            //parse the jwt to get the username
            string bearerToken = bearerTokenHeader.ToString().Substring(7);
            string username = Helpers.GetUsernameFromJWT(bearerToken);

            var task = new ExtImplantTask_Base( request.Command, request.Arguments, request.File, request.IsBlocking, request.RequiresPreProc, request.RequiresPostProc, username, implantId);

            if (! await implant.QueueTask(task))
            {
                ThrowError("failed to queue task");
            }
           
            HardHatHub.StoreTaskHeader(task);
            // add the implantId as the key with a new list of ImplantTask to the Implant.Previous task Dictionary and add the task to the list of tasks for the implant
            ExtImplantTask_Base LoggingTask = new ExtImplantTask_Base()
            {
                Id = task.Id,
                Command = task.Command,
                Arguments = task.Arguments,
                File = null,
                IsBlocking = task.IsBlocking,
                RequiresPostProc = task.RequiresPostProc,
                RequiresPreProc = task.RequiresPreProc,
                TaskHeader = task.TaskHeader,
                IssuingUser = task.IssuingUser,
                ImplantId = task.ImplantId,
            };

            HardHatHub.UpdateOutgoingTaskDic(implant, new List<ExtImplantTask_Base>() { task }, "");
            var args = task.Arguments is null ? "" : string.Join(" ", task.Arguments.Select(kvp => $"{kvp.Key} {kvp.Value}"));
            HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"task {task.Command} {args} queued for execution", Status = "info" });
            LoggingService.TaskLogger.ForContext("Task", LoggingTask, true).ForContext("Implant_Id", implantId).Information($"task {task.Command} queued for execution, NOTE: file property is always null in logs");
            SendOkAsync(task);
        }
    }
}
