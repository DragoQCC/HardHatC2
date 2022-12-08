using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Models.Extras;
using TeamServer.Services;

namespace TeamServer.Utilities
{
    public class TaskPostProcess
    {

        public static async Task PostProcessTask()
        {
            

        }
        
        public static async Task PostProcess_CredTask(EngineerTaskResult result)
        {
            var CredsList = new List<Cred>();
            
            var capturedCreds = CapturedCredential.ParseCredentials(result.Result);
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
                await HardHatHub.AddCreds(CredsList);
            }
        }
     
        public static async Task PostProcess_DownloadTask(EngineerTaskResult result)
        {
            var finalb64 = "";
            var base64 = result.Result;
            //extract the section from the end of the base64 string it will be in the format of "PARTint/total" take the number between the string PART and the / 
            var sectionString = base64.Substring(base64.IndexOf("PART"), base64.LastIndexOf("/") - base64.IndexOf("PART"));
            sectionString = sectionString.Remove(0, 4);
            var section = int.Parse(sectionString);
            //extract the total from the end of the base64 string it will be in the format of "Section 1/2" take the number to the right of the / until the end of the string
            var totalString = base64.Substring(base64.LastIndexOf("/"));
            totalString = totalString.Remove(0, 1);
            var total = int.Parse(totalString);
            HttpmanagerController.CommandIds[result.Id].Arguments.TryGetValue("/file", out string filename);
            // get the last word in the filename string and update filename to that
            var fileNameSplit = filename.Split("\\").Last();


            //take the base64 string and remove the last 2 characters which are the section and total, put all the base64 strings together and the when section == total then write the file to disk
            var Cleanedbase64 = base64.Substring(0, base64.IndexOf("PART"));
            finalb64 += Cleanedbase64;

            if (section + 1 != total)
            {
                //remove result from results 
                HttpmanagerController.results = HttpmanagerController.results.Where(val => val != result);
            }
            if (section + 1 == total)
            {
                char allPlatformPathSeperator = Path.DirectorySeparatorChar;
                // find the Engineer cs file and load it to a string so we can update it and then run the compiler function on it
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //split path at bin keyword
                string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
                                                        //update each string in the array to replace \\ with the correct path seperator
                pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());

                System.IO.File.WriteAllBytes(pathSplit[0] + "Downloads" + $"{allPlatformPathSeperator}{fileNameSplit}", Convert.FromBase64String(finalb64));
                HttpmanagerController.CommandIds.Remove(result.Id);
                result.Result = "Successfully downloaded file check the downloads folder on the ts or the downloads tab on the client to sync up.";
                DownloadFile file = new DownloadFile();
                file.Name = fileNameSplit;
                file.OrginalPath = filename;
                file.SavedPath = pathSplit[0] + "Downloads" + $"{allPlatformPathSeperator}{fileNameSplit}";
                await HardHatHub.AlertDownload(file);
            }
        }
         
        public static async Task PostProcess_SocksTask(EngineerTaskResult result)
        {
            try
            {
                //if result.Id is socksConnected then split the incoming result string into an array and element 1 is the client unique string and we need to update the Proxy.SocksDestinationConnected
                if (result.Id.Equals("ConnectSocksCommand", StringComparison.CurrentCultureIgnoreCase))
                {
                    var socksConnected = result.Result.Split(new[] { "\n" }, StringSplitOptions.None);
                    //element 1 in the array matches a key in the SocksDestinationConnected dictionary update the value to true 
                    HttpmanagerController.Proxy.SocksDestinationConnected[socksConnected[1]] = true;
                }

                //if result.Id is socksReceiveData then set the GotData in Socks4Proxy to true and convert the result.Result from base64 string to byte[] and assign it to the socks4Proxy resp
                else if (result.Id.Equals("socksReceiveData", StringComparison.CurrentCultureIgnoreCase))
                {
                    //split result.Result at the new line 
                    var split = result.Result.Split(new[] { "\n" }, StringSplitOptions.None);
                    byte[] temp = Convert.FromBase64String(split[0]); //split 0 is the data split 1 is the guid
                    string client = split[1];
                    Console.WriteLine($"teamserver received {temp.Length} bytes from {client}");
                    HttpmanagerController.Proxy.SocksClientsData[client].Enqueue(temp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task PostPorcess_RPortForward(EngineerTaskResult result)
        {
            //if result.Id is rportsend take the result.Result and split it at the new line and element 0 is the data and element 1 is the guid
            if (result.Id.Equals("rportsend", StringComparison.CurrentCultureIgnoreCase))
            {
                var split = result.Result.Split(new[] { "\n" }, StringSplitOptions.None);
                byte[] temp = Convert.FromBase64String(split[0]); //split 0 is the data split 1 is the guid
                string client = split[1];
                Console.WriteLine($"teamserver received {temp.Length} bytes from {client}");
                RPortForward.RPortForwardClientsData[client].Enqueue(temp);
            }

        }

        public static async Task<string> PostProcess_P2PFirstCheckIn(EngineerTaskResult result, Engineer HttpEng)
        {
            string[] resultArray = result.Result.Split('\n');
            string Base64Metadata = resultArray[0];
            string parentId = resultArray[1];
            //if(HttpmanagerController.EngineerChildIds.ContainsKey(parentId))
            //{
            //    HttpmanagerController.EngineerChildIds[parentId].Add(result.EngineerId);
            //}
            //else
            //{
            //    HttpmanagerController.EngineerChildIds.Add(parentId, new List<string> { result.EngineerId });
            //}
            if(!HttpmanagerController.PathStorage.ContainsKey(result.EngineerId))
            {
                // new engineer, key is its id, value is its path, if its parent id is equal to the http engineer then just the http engineer id is its path, otherwise check if its parent is in the HttpmanagerController.PathStorage 
                if(parentId == HttpEng.engineerMetadata.Id)
                {
                    Console.WriteLine($"adding new path of {HttpEng.engineerMetadata.Id} -> {result.EngineerId}");
                    HttpmanagerController.PathStorage.Add(result.EngineerId, new List<string> { HttpEng.engineerMetadata.Id,result.EngineerId });
                }
                else
                {
                    foreach (var kvp in HttpmanagerController.PathStorage)
                    {
                        if (kvp.Key == parentId)
                        {
                            var path = kvp.Value;
                            // this should end up making it easy to have path regradless of depth
                            //add to HttpmanagerController.PathStorage the result.EngineerId as the key and the value is each element in the path list and the result.EngineerId
                            Console.WriteLine($"adding new path of {string.Join("->", path)} -> {result.EngineerId}");
                            var temp = new List<string>();
                            path.ForEach(x => temp.Add(x));
                            HttpmanagerController.PathStorage.Add(result.EngineerId,temp);
                            HttpmanagerController.PathStorage[result.EngineerId].Add(result.EngineerId); // I was adding path here and then adding to it which updated path in both spots.
                            break;
                        }
                    }
                }
            }
            return Base64Metadata;
        }
    }
}
