using System;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class MyCustomCommand : EngineerCommand
    {
        public override string Name => "MyCustomCommand";

        public override async Task Execute(EngineerTask task)
        {
            Console.WriteLine();
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("hello from custom command", task, EngTaskStatus.Complete,TaskResponseType.String);
            return;
        }
    }
}
