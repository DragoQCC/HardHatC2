using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;

namespace Engineer.Commands
{
    internal class socks : EngineerCommand
    {
        public static List<string> SocksClients = new();
        public static readonly  ConcurrentDictionary<string, ConcurrentQueue<byte[]>> SocksClientsData = new();
        //public static readonly  ConcurrentDictionary<string, bool> SocksClientsGotData = new();
       // public static readonly  ConcurrentDictionary<string, bool> SocksClientWaiting = new();
 

        public override string Name => "socks";
        internal static CancellationTokenSource _tokenSource = new();
        public override string Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/port", out var port);
            //if task.arguments holds key /stop return saying the socks proxy won /port was stopped
            if (task.Arguments.ContainsKey("/stop"))
            {
                return $"Socks proxy on port {port} stopped";
            }

            return $"socks started on team server at port {port}";

        }
    }
    internal class socksConnect : EngineerCommand
    {
        public override string Name => "socksConnect";

        public override string Execute(EngineerTask task)
        {
            try
            {
                //try to get the values for /Address and /Port from the task.arguments dictionary convert the address to an IpAddress object and the port to an int
                task.Arguments.TryGetValue("/Address", out var address);
                task.Arguments.TryGetValue("/Port", out var port);
                task.Arguments.TryGetValue("/Client", out var client);
                socks.SocksClients.Add(client);
                socks.SocksClientsData.TryAdd(client, new ConcurrentQueue<byte[]>());
                //socks.SocksClientsGotData.TryAdd(client, false);
                //Console.WriteLine($"Connecting to {address}:{port}");

                Task.Run(async () => await ConnectSocks(address, port, client));
                return $"Connecting to {address}:{port}"+$"\n{client}";
            }
            
            catch(Exception e)
            {
               // Console.WriteLine(e.Message);
               // Console.WriteLine(e.StackTrace);
                return "error with socks connect";
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
                while (!socks._tokenSource.IsCancellationRequested)
                {
                    
                    //if destination is not connected remove it from the socks clients list and dictionarys and exit while loop
                    if (!destination.Connected)
                    {
                        socks.SocksClients.Remove(client);
                        socks.SocksClientsData.TryRemove(client, out var _);
                        break;
                    }
                    
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
                        Console.WriteLine("sending teamerver " + resp.Length + " bytes " + $"for client {client}");
                        //make a new engineer task with the argument /data which is resp converted to a base64 string and then add the task to the queue
                        var task = new EngineerTask
                        {
                            Id = "socksReceiveData",
                            Command = "socksReceive",
                            File = resp,
                            Arguments = new Dictionary<string, string>
                        {
                            {"/client",client }
                        }
                        };
                       Program.InboundCommandsRec += 1;
                       Task.Run(async() => await Program.DealWithTask(task));
                    }
                    // rip cpu
                    await Task.Delay(10);
                }
            }
            catch (Exception e)
            {
               //Console.WriteLine(e.Message);
              // Console.WriteLine(e.StackTrace);
            }
        }
    }
    
    internal class SocksSend : EngineerCommand
    {
        public override string Name => "socksSend";

        public override string Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/client", out var client);

            //while the socks client is waiting for data to be sent do not send the data 
            var req = task.File;
            socks.SocksClientsData[client].Enqueue(req);
            //socks.SocksClientsGotData[client] = true;
            return $"Sending data";
        }
    }
    
    internal class SocksReceive : EngineerCommand
    {
        public override string Name => "socksReceive";

        public override string Execute(EngineerTask task)
        {
            // trygetvalue of task.arguments /data and return that value
            task.Arguments.TryGetValue("/client", out var client);
            return Convert.ToBase64String(task.File)+"\n"+client;
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
    }
}
