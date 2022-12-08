using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class Connect : EngineerCommand
    {
        public override string Name => "Connect";
        public EngTCPComm ParentTCPcommModule { get; set;}
        public bool ParentIsServer { get; set;}
        public static string Output { get; set;}

        public override string Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/port", out var serverport);
            task.Arguments.TryGetValue("/ip", out var serverip);
            task.Arguments.TryGetValue("/localhost", out var isLocalHost); //sets if the server should listen on only localhost or on 0.0.0.0

            serverport = serverport.TrimStart(' ');
            if (serverip != null)
            {
                serverip = serverip.TrimStart(' ');
            }
            if (isLocalHost != null)
            {
                isLocalHost = isLocalHost.TrimStart(' ');
            }

            if (task.Arguments.ContainsKey("/localhost"))
            {
                ParentIsServer = true;
            }
            else
            {
                ParentIsServer = false;
            }

            if (ParentIsServer)
            {
                Console.WriteLine("starting parent as server");
                ParentTCPcommModule = new EngTCPComm(int.Parse(serverport), bool.Parse(isLocalHost), true); // parent as server
                Task.Run(async()=> await ParentTCPcommModule.Start());                                                                                            
            }
            else if (!ParentIsServer)
            {
                Console.WriteLine("starting parent as client");
                Console.WriteLine($"trying to connect to {serverip}:{serverport}");
                ParentTCPcommModule = new EngTCPComm(int.Parse(serverport), serverip, true); // parent as client
                Task.Run(async () => await ParentTCPcommModule.Start());
            }
            while (Output == null)
            {
                System.Threading.Thread.Sleep(20);
            }
            return Output;
        }
    }
}
