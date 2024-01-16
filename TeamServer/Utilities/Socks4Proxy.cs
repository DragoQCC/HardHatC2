using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;


namespace HardHatCore.TeamServer.Utilities
{
    public class Socks4Proxy
    {
        private readonly int _bindPort;
        private readonly IPAddress _bindAddress;
        private readonly CancellationTokenSource _tokenSource = new();
        public static int TrafficSize = 12800000;

        private ConcurrentDictionary<string,TcpClient> SocksClients = new();
        public ConcurrentDictionary<string, ConcurrentQueue<byte[]>> SocksClientsData = new();
        public ConcurrentDictionary<string, bool> SocksDestinationConnected = new();
        
        public Socks4Proxy(IPAddress bindAddress = null, int bindPort = 1080)
        {
            _bindPort = bindPort;
            _bindAddress = bindAddress ?? IPAddress.Any;
        }

        public async Task Start(ExtImplant_Base implant)
        {
            try
            {
                var listener = new TcpListener(_bindAddress, _bindPort);
                listener.Start(500);
                while (!_tokenSource.IsCancellationRequested)
                {
                    // this blocks until a connection is received
                    var client = await listener.AcceptTcpClientAsync(_tokenSource.Token);
                    //ensures the network stream buffer is always a bit larger than the traffic size
                    client.ReceiveBufferSize = TrafficSize;
                    client.SendBufferSize = TrafficSize;
                    client.NoDelay = true;
                    Task.Run(async () => await HandleClient(client, implant));
                    
                }

                listener.Stop();
                listener.Server.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        private async Task HandleClient(TcpClient client, ExtImplant_Base implant)
        {
            // read connect request making sure connection type is socks 4 and that the client command is to stream data 
            var request = await ReadConnectRequest(client);

            if (request is null)
            {
                Console.WriteLine("Socks Request is null");
                return;
            }
            //add or update the client in the dictionary
            string client_guid = Guid.NewGuid().ToString();
            SocksClients.TryAdd(client_guid, client);
            SocksClientsData.TryAdd(client_guid, new ConcurrentQueue<byte[]>());
            SocksDestinationConnected.TryAdd(client_guid, false);
            IExtimplantHandleComms.SocksClientToProxyCache.TryAdd(client_guid, _bindPort.ToString());
            Dictionary<string, byte[]> sockConnectNotifArgs = new Dictionary<string, byte[]>()
            {
                { "/Address", request.DestinationAddress.ToString().Serialize() },
                { "/Port", request.DestinationPort.ToString().Serialize() },
                { "/Client", client_guid.Serialize() }
            };
            //make an implant task to connect to the client send task name of ConnectSocks with arguments /address and /port which are the request.DestinationAddress, request.DestinationPort
            var assetNotifService = PluginService.GetAssetNotifService(implant.ImplantType);
            // add the task to the implants task queue
            await assetNotifService.CreateAndQueueAssetNotif(implant.Metadata.Id, "SocksConnect", sockConnectNotifArgs);  

            // wait for the implant to connect to the client
            while (!SocksDestinationConnected[client_guid])
            {
                await Task.Delay(2);
            }

            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    // read from client
                    if (client.DataAvailable())
                    {
                        var req = await client.ReceiveData(_tokenSource.Token);
                        var sockSendNotifArgs = new Dictionary<string, byte[]>
                        {
                            {"/client", client_guid.Serialize()},
                            {"/data", req}
                        };
                        await assetNotifService.CreateAndQueueAssetNotif(implant.Metadata.Id, "SocksSend", sockSendNotifArgs);
                    }

                    // in a thread safe way find the clients with data to send and send it if the client is still connected
                    if (!SocksClientsData[client_guid].IsEmpty)
                    {
                        SocksClientsData[client_guid].TryDequeue(out var data);
                        await client.SendData(data, _tokenSource.Token);
                    }

                    // rip cpu
                    await Task.Delay(1);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
        }

        
        private async Task<Socks4Request> ReadConnectRequest(TcpClient client)
        {
            // read data from client
            var data = await client.ReceiveData(_tokenSource.Token);

            if (data.Length == 0)
                return null;

            // read the first byte, which is the SOCKS version
            var version = Convert.ToInt32(data[0]);

            // check version
            if (version == 4)
            {
                var request = await Socks4Request.FromBytes(data);

                // check command
                if (request.Command == Socks4Request.CommandCode.StreamConnection)
                {
                    await SendConnectReply(client, true);
                    return request;
                }
            }

            // otherwise send an error
            await SendConnectReply(client, false);
            return null;
        }

        private async Task SendConnectReply(TcpClient client, bool success)
        {
            var reply = new byte[]
            {
            0x00,
            success ? (byte)0x5a : (byte)0x5b,
            0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
            };

            await client.SendData(reply, _tokenSource.Token);
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            
        }
    }

    
    internal class Socks4Request
    {
        public CommandCode Command { get; private init; }
        public int DestinationPort { get; private init; }
        public IPAddress DestinationAddress { get; private set; }

        public static async Task<Socks4Request> FromBytes(byte[] raw)
        {
            var request = new Socks4Request
            {
                Command = (CommandCode)raw[1],
                DestinationPort = raw[3] | raw[2] << 8,
                DestinationAddress = new IPAddress(raw[4..8])
            };

            // if this is SOCKS4a
            if (request.DestinationAddress.ToString().StartsWith("0.0.0."))
            {
                var domain = Encoding.UTF8.GetString(raw[9..]);
                var lookup = await Dns.GetHostAddressesAsync(domain);

                // get the first ipv4 address
                request.DestinationAddress = lookup.First(i => i.AddressFamily == AddressFamily.InterNetwork);
            }

            return request;
        }

        public enum CommandCode : byte
        {
            StreamConnection = 0x01,
            PortBinding = 0x02
        }
    }

    
    public class ProxyState
    {
        public byte[] Buffer;
        public readonly Socket Client;
        public readonly Socket Destination;

        public ProxyState(Socket client, Socket destination)
        {
            Client = client;
            Destination = destination;
            Buffer = new byte[Socks4Proxy.TrafficSize];
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
                var buf = new byte[Socks4Proxy.TrafficSize];
                read = await ns.ReadAsync(buf, 0, buf.Length, token);

                if (read == 0)
                    break;

                await ms.WriteAsync(buf, 0, read, token);

            } while (read >= Socks4Proxy.TrafficSize);

            return ms.ToArray();
        }

        public static async Task SendData(this TcpClient client, byte[] data, CancellationToken token)
        {
            if (client.Connected)
            {
                var ns = client.GetStream();
                await ns.WriteAsync(data, 0, data.Length, token);
            }
        }

        public static bool DataAvailable(this TcpClient client)
        {
            if (client.Connected)
            {
                var ns = client.GetStream();
                return ns.DataAvailable;
            }
            return false;
        }
    }

}
