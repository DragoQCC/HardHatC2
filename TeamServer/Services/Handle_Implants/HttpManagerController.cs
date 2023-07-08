using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using TeamServer.Utilities;
using TeamServer.Models;


/* used to interact with http managers that are created
 * these are hosted by the application which is why this controller looks diff from the others  those control interactions with the API/teamserver backend itself
 * Still responsible for any IActionResult stuff from web based interactions 
 */

namespace TeamServer.Services.Handle_Implants
{
    [Controller]
    public class HttpmanagerController : ControllerBase
    {
        private readonly IEngineerService _engineers;

        public static Dictionary<string, Socks4Proxy> Proxy { get; set; } = new Dictionary<string, Socks4Proxy>();
        //public static Socks4Proxy testProxy { get; set; }
        //public static Dictionary<string, EngineerTask> CommandIds = new Dictionary<string, EngineerTask>();
        //public static Dictionary<string, List<string>> EngineerChildIds = new Dictionary<string, List<string>>(); //keys are Engineers that have child engineers they can task, value is the childrens ids  
        // public static Dictionary<string, List<string>> PathStorage = new Dictionary<string, List<string>>(); // key is the Engineer Id, Value is a list of parent ids and ends with its own id, making its path. The path is a list element 0 is the http, and each new eleemnt is a layer deepr
        // public static Dictionary<string,int> EngineerCheckinCount = new Dictionary<string, int>(); //key is the engineer id, value is the number of times they have checked in
        // public static IEnumerable<EngineerTaskResult> results { get; set;}


        public HttpmanagerController(IEngineerService engineers) //uses dependdency Injection to link  to service and crerate object instance. 
        {
            _engineers = engineers;
        }


        public async Task<IActionResult> HandleImplantAsync()                // the http tags are not always needed with IActionResults 
        {
            try
            {
                string implant_name = GetImplantType(HttpContext.Request.Headers);
                // by this point we have gotten back data from the eng either for a check in or a task response.
                // DeEncrypt the HttpRequest using the Encryption.AES_Decrypt function
                if (implant_name.Equals("Engineer", StringComparison.InvariantCultureIgnoreCase))
                {
                    EngineerMetadata engineermetadata = ExtractMetadata<EngineerMetadata>(HttpContext.Request.Headers, implant_name);
                    if (engineermetadata is null)
                    {
                        Console.WriteLine("No metadata found");
                        return NotFound();
                    }
                    Handle_Engineer handle_Engineer = new Handle_Engineer(_engineers);
                    return await handle_Engineer.HandleEngineerAsync(engineermetadata, HttpContext);
                }
                else if (implant_name.Equals("Constructor", StringComparison.InvariantCultureIgnoreCase))
                {
                    //engineermetadata = ExtractMetadata<ConstructorMetadata>(HttpContext.Request.Headers, implant_name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
            return BadRequest();
        }

        private string GetImplantType(IHeaderDictionary headers)
        {
            try
            {
                if (!headers.TryGetValue("Authorization", out var encryptedencodedMetadata))     //extracted as base64 due too TryGetValue
                {
                    Console.WriteLine("no authorization header");
                    return null;
                }
                // Console.WriteLine($"metadata encryption key is {Encryption.UniqueMetadataKey[decryptedImplantId]}");
                encryptedencodedMetadata = encryptedencodedMetadata.ToString().Remove(0, 7);           // cleans up the from out the `Authorization: Bearer METADATAHERE`  response 
                                                                                                       // DeEncrypt the metadata using the Encryption.AES_Decrypt function
                                                                                                       //we need to extract the length and the implant name from the metadata it will be the bytes after the length bytes 
                                                                                                       //the length bytes will be the first 4 bytes of the metadata
                                                                                                       //the implant name will be the bytes after the length bytes
                int length = BitConverter.ToInt32(Convert.FromBase64String(encryptedencodedMetadata).Take(4).ToArray(), 0);
                string XORED_implantName = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedencodedMetadata).Skip(4).Take(length).ToArray());
                string implant_name = Encryption.DecryptImplantName(XORED_implantName);
                if(implant_name == "")
                {
                    implant_name = "Engineer";
                }
                return implant_name;

            }
            catch (Exception ex)
            {
                //DEBUG REMOVE
                Console.WriteLine("Failed to extract implant name");
                return "Engineer";
            }  
        }

        private T ExtractMetadata<T>(IHeaderDictionary headers, string implant_name)
        {
            try
            {
                if (!headers.TryGetValue("Authorization", out var encryptedencodedMetadata))     //extracted as base64 due too TryGetValue
                {
                    Console.WriteLine("no authorization header");
                    return default;
                }
                // Console.WriteLine($"metadata encryption key is {Encryption.UniqueMetadataKey[decryptedImplantId]}");
                encryptedencodedMetadata = encryptedencodedMetadata.ToString().Remove(0, 7);           // cleans up the from out the `Authorization: Bearer METADATAHERE`  response 
                int length = BitConverter.ToInt32(Convert.FromBase64String(encryptedencodedMetadata).Take(4).ToArray(), 0);
                //remove the length bytes and the implant name bytes from the metadata
                encryptedencodedMetadata = Convert.ToBase64String(Convert.FromBase64String(encryptedencodedMetadata).Skip(4 + length).ToArray());
                if (implant_name.Equals("Engineer", StringComparison.InvariantCultureIgnoreCase))
                {
                    byte[] encodedMetadataArray = Encryption.Engineer_AES_Decrypt(Convert.FromBase64String(encryptedencodedMetadata), Encryption.UniversialMetadataKey);
                    return encodedMetadataArray.Deserialize<T>(); // deserialise the metadata
                }
                else if (implant_name.Equals("Constructor", StringComparison.InvariantCultureIgnoreCase))
                {
                    byte[] encodedMetadataArray = Encryption.Engineer_AES_Decrypt(Convert.FromBase64String(encryptedencodedMetadata), Encryption.UniversialMetadataKey);
                    return encodedMetadataArray.Deserialize<T>(); // deserialise the metadata
                }
                else
                {
                    return default; // if the implant name is not engineer or constructor then we dont know what it is and we dont want it
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return default;
            }
        }
    }
}
