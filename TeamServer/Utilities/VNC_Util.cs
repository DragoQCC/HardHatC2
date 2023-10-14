using ApiModels.Shared;
using ApiModels.Shared.TaskResultTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TeamServer.Utilities
{
    public class VNC_Util
    {
        //key is the sessionID
        public static Dictionary<string, VNCSessionMetadata> ImplantVNCSessionMeta = new Dictionary<string, VNCSessionMetadata>();
        public static Dictionary<string, VncInteractionResponse> VNCSessionResponse = new Dictionary<string, VncInteractionResponse>();

        public static async Task InitOrUpdateSession(VNCSessionMetadata meta, VncInteractionResponse response)
        {
            try
            {
                if (ImplantVNCSessionMeta.ContainsKey(meta.SessionID))
                {
                    ImplantVNCSessionMeta[meta.SessionID] = meta;
                    VNCSessionResponse[meta.SessionID] = response;
                }
                else
                {
                    ImplantVNCSessionMeta.Add(meta.SessionID, meta);
                    VNCSessionResponse.Add(meta.SessionID, response);
                }

            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}
