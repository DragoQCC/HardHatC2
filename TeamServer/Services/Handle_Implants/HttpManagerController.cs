using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Net.Http;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Utilities;


/* used to interact with http managers that are created
 * these are hosted by the application which is why this controller looks diff from the others  those control interactions with the API/teamserver backend itself
 * Still responsible for any IActionResult stuff from web based interactions
 */

namespace HardHatCore.TeamServer.Services.Handle_Implants
{
    [Controller]
    public class HttpmanagerController : ControllerBase
    {
        //private readonly IEngineerService _engineers;

        public static Dictionary<string, Socks4Proxy> Proxy { get; set; } = new Dictionary<string, Socks4Proxy>();
        public static Dictionary<string,string> SocksClientToProxyCache { get; set; } = new Dictionary<string, string>();


        // the http tags are not always needed with IActionResults 
        public async Task<IActionResult> HandleImplantAsync() 
        {
            try
            {
                string implant_name = GetImplantType(HttpContext.Request.Headers);
                // by this point we have gotten back data from the eng either for a check in or a task response.
                // DeEncrypt the HttpRequest using the Encryption.AES_Decrypt function
                var implantMetadata = ExtractMetadata<ExtImplantMetadata_Base>(HttpContext.Request.Headers, implant_name);
                var comm_base = PluginService.GetImpCommsPlugin(implant_name);
                if (implantMetadata != null)
                {
                    ExtImplantService_Base extImplantService_Base = Plugin_Management.PluginService.GetImpServicePlugin(implant_name);
                    ExtImplant_Base implant = await comm_base.GetCheckingInImplant(implantMetadata, HttpContext , extImplantService_Base, implant_name);
                    //make a copy of the context so we can use it even after the request has been handled
                    //var _httpContext = await CaptureData(HttpContext);
                    //handle the implant request and respond to it
                    await comm_base.HandleImplantRequest(implant, extImplantService_Base, HttpContext);
                    return await comm_base.RespondToImplant(implant, extImplantService_Base);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
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
                // cleans up the from out the `Authorization: Bearer METADATAHERE`  response 
                encryptedencodedMetadata = encryptedencodedMetadata.ToString().Remove(0, 7);           
                //we need to extract the length and the implant name from the metadata it will be the bytes after the length bytes 
                //the length bytes will be the first 4 bytes of the metadata
                int length = BitConverter.ToInt32(Convert.FromBase64String(encryptedencodedMetadata).Take(4).ToArray(), 0);
                //the implant name will be the bytes after the length bytes
                string XORED_implantName = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedencodedMetadata).Skip(4).Take(length).ToArray());
                //Console.WriteLine($"XORED implant name is {XORED_implantName}");
                string implant_name = Encryption.DecryptImplantName(XORED_implantName);
                //Console.WriteLine($"Implant name is {implant_name}");
                if (implant_name == "")
                {
                    #if DEBUG
                    Console.WriteLine("Failed to extract implant name, debugging, setting to Engineer");
                    implant_name = "Engineer";
                    #endif
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

        //need to move this into the implant service so it can be overriden I have the name so i can find the plugins
        private T ExtractMetadata<T>(IHeaderDictionary headers, string implant_name)
        {
            try
            {
                if (!headers.TryGetValue("Authorization", out var encryptedencodedMetadataWithName))     //extracted as base64 due too TryGetValue
                {
                    Console.WriteLine("no authorization header");
                    return default;
                }
                // cleans up the from out the `Authorization: Bearer METADATAHERE`  response 
                encryptedencodedMetadataWithName = encryptedencodedMetadataWithName.ToString().Remove(0, 7);
                
                //remove the length bytes and the implant name bytes from the metadata
                int length = BitConverter.ToInt32(Convert.FromBase64String(encryptedencodedMetadataWithName).Take(4).ToArray(), 0);
                byte[] encryptedencodedMetadata = Convert.FromBase64String(encryptedencodedMetadataWithName).Skip(4 + length).ToArray();
                
                //find the correct plugin to decrypt the metadata
                var extImplantService_Base = PluginService.GetImpServicePlugin(implant_name);
                byte[] encodedMetadataArray = extImplantService_Base.DecryptImplantTaskData(encryptedencodedMetadata, Encryption.UniversialMetadataKey);
                if(encodedMetadataArray == null)
                {
                    return default;
                }
                // deserialise the metadata
                var metadata = encodedMetadataArray.Deserialize<T>(); 
                if (metadata == null)
                {                     
                    Console.WriteLine("Failed to extract metadata");
                    return default;
                }
                return metadata;
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
