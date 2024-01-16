using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_BaseClasses;

//using DynamicEngLoading;

namespace HardHatCore.TeamServer.Utilities
{
    public class RPortForward
    {
        private readonly int _bindPort;
        private readonly IPAddress _bindAddress;
        private static ConcurrentDictionary<string,CancellationTokenSource> _tokenSources = new();

        public static ConcurrentDictionary<TcpClient, string> RPortForwardClients = new();
        public static ConcurrentDictionary<string, ConcurrentQueue<byte[]>> RPortForwardClientsData = new();
        public static ConcurrentDictionary<string, bool> RPortForwardDestinationConnected = new();
        public static ConcurrentDictionary<string, List<string>> RPortForwardEngineersClients = new(); // Engineer Id is key, client ids for that implant are the values

        //run this as a pre process command option, so the team server can make a tcpclient for this target and be ready to send and recive data from it
        public static async Task rPortStart(string targetAddress, int targetPort, string clientId, ExtImplant_Base implant)
        {
            //make a tcpclient with the address and port, then update the Dictionaries with the clientId
            var client = new TcpClient();
            RPortForwardClients.TryAdd(client, clientId);
            RPortForwardClientsData.TryAdd(clientId, new ConcurrentQueue<byte[]>());
            RPortForwardDestinationConnected.TryAdd(clientId, false);
            _tokenSources.TryAdd(clientId, new CancellationTokenSource());
            Console.WriteLine($"client {clientId} added to rportforward");
            if (RPortForwardEngineersClients.ContainsKey(implant.Metadata.Id))
            {
                RPortForwardEngineersClients[implant.Metadata.Id].Add(clientId);
            }
            else
            {
                RPortForwardEngineersClients.TryAdd(implant.Metadata.Id, new List<string>() { clientId });
                Console.WriteLine($"implant {implant.Metadata.Id} added to rportforward");
            }
            Task.Run(async() => await HandleComms(client, implant,targetAddress,targetPort));
        }

        public static async Task HandleComms(TcpClient client, ExtImplant_Base implant, string targetAddress, int targetPort)
        {
            //check if targetAddress is in the format of an ip address and if not try to resolve the dns name to an ip
            if (!IPAddress.TryParse(targetAddress, out var targetIp))
            {
                targetIp = (await Dns.GetHostAddressesAsync(targetAddress))[0];
            }

            // make a while loop that will run and check the RPortForwardClientsData queue and if it has data in it update the RPortForwardDestinationConnected to true, send the data and put it back to false
            while (true)
            {
                // handles sending data to the client from the implant
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
                // should handle taking data from the client and making a task to send the implant
                if(RPortForwardDestinationConnected[RPortForwardClients[client]])
                {
                    //check if client has data and if it does build a rportRecieve task to send to the implant
                    if (client.DataAvailable())
                    {
                        var dataForDestionation = await client.ReceiveData(_tokenSources[RPortForwardClients[client]].Token);
                        var task = new ExtImplantTask_Base("rportRecieve",new Dictionary<string, string>(){{"/client", RPortForwardClients[client] }}, dataForDestionation, false,false,true, "",implant.Metadata.Id);
                        implant.QueueTask(task);
                    }
                }
                await Task.Delay(10);
            }
        }

    }

    
}
