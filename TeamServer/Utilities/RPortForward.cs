using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TeamServer.Models;
using System.Collections.Generic;
using System.IO;
using System;

namespace TeamServer.Utilities
{
    public class RPortForward
    {
        private readonly int _bindPort;
        private readonly IPAddress _bindAddress;
        private static ConcurrentDictionary<string,CancellationTokenSource> _tokenSources = new();

        public static ConcurrentDictionary<TcpClient, string> RPortForwardClients = new();
        public static ConcurrentDictionary<string, ConcurrentQueue<byte[]>> RPortForwardClientsData = new();
        public static ConcurrentDictionary<string, bool> RPortForwardDestinationConnected = new();
        public static ConcurrentDictionary<string, List<string>> RPortForwardEngineersClients = new(); // Engineer Id is key, client ids for that engineer are the values

        //run this as a pre process command option, so the team server can make a tcpclient for this target and be ready to send and recive data from it
        public static async Task rPortStart(string targetAddress, int targetPort, string clientId, Engineer engineer)
        {
            //make a tcpclient with the address and port, then update the Dictionaries with the clientId
            var client = new TcpClient();
            RPortForwardClients.TryAdd(client, clientId);
            RPortForwardClientsData.TryAdd(clientId, new ConcurrentQueue<byte[]>());
            RPortForwardDestinationConnected.TryAdd(clientId, false);
            _tokenSources.TryAdd(clientId, new CancellationTokenSource());
            Console.WriteLine($"client {clientId} added to rportforward");
            if (RPortForwardEngineersClients.ContainsKey(engineer.engineerMetadata.Id))
            {
                RPortForwardEngineersClients[engineer.engineerMetadata.Id].Add(clientId);
            }
            else
            {
                RPortForwardEngineersClients.TryAdd(engineer.engineerMetadata.Id, new List<string>() { clientId });
                Console.WriteLine($"engineer {engineer.engineerMetadata.Id} added to rportforward");
            }
            Task.Run(async() => await HandleComms(client, engineer,targetAddress,targetPort));
        }

        public static async Task HandleComms(TcpClient client, Engineer engineer, string targetAddress, int targetPort)
        {
            //check if targetAddress is in the format of an ip address and if not try to resolve the dns name to an ip
            if (!IPAddress.TryParse(targetAddress, out var targetIp))
            {
                targetIp = (await Dns.GetHostAddressesAsync(targetAddress))[0];
            }

            // make a while loop that will run and check the RPortForwardClientsData queue and if it has data in it update the RPortForwardDestinationConnected to true, send the data and put it back to false
            while (true)
            {
                // handles sending data to the client from the engineer
                if (RPortForwardClientsData[RPortForwardClients[client]].TryDequeue(out var dataForClient))
                {
                    if (!client.Connected)
                    {
                        await client.ConnectAsync(targetIp, targetPort);
                    }
                    if (client.Connected)
                    {
                        RPortForwardDestinationConnected[RPortForwardClients[client]] = true;
                        await client.SendData(dataForClient, _tokenSources[RPortForwardClients[client]].Token);
                    }
                }
                // should handle taking data from the client and making a task to send the engineer
                if(RPortForwardDestinationConnected[RPortForwardClients[client]])
                {
                    //check if client has data and if it does build a rportRecieve task to send to the engineer
                    if (client.DataAvailable())
                    {
                        var dataForDestionation = await client.ReceiveData(_tokenSources[RPortForwardClients[client]].Token);
                        var task = new EngineerTask("rportRecieve", "rportRecieve",new Dictionary<string, string>(){{"/client", RPortForwardClients[client] }}, dataForDestionation, false);
                        engineer.QueueTask(task);
                    }
                }
                await Task.Delay(10);
            }
        }

    }

    
}
