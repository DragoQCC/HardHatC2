using Engineer.Commands;
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

        public override string Execute(EngineerTask task)
        {
            var identity = WindowsIdentity.GetCurrent();
            string Username = identity.Name;
            if (task.Arguments != null)
            {
                if (task.Arguments.ContainsKey("/groups"))
                {
                    var groups = identity.Groups;
                    var groupNames = groups.Select(g => g.Value);
                    return $"{Username} is a member of the following groups: {string.Join(", ", groupNames)}";
                }
            }
            return $"{Username}";
        }
    }
}
