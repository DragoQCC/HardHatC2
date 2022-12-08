using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class print_env : EngineerCommand
    {
        public override string Name => "print-env";

        public override string Execute(EngineerTask task)
        {
            //get all of the enviornment variables and return them one entry per line
            var output = new StringBuilder();
            foreach (var env in System.Environment.GetEnvironmentVariables().Keys)
            {
                output.AppendLine(env.ToString() + ": " + System.Environment.GetEnvironmentVariable(env.ToString()));
            }
            return output.ToString();

        }
    }
}
