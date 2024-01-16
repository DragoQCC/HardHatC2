using HardHatCore.ApiModels.Shared;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using HardHatCore.HardHatC2Client.Components;


namespace HardHatCore.HardHatC2Client.Utilities
{
    public class VNC_Util
    {
        public static Dictionary<string, VNCSessionMetadata> ImplantVNCSessionMeta = new Dictionary<string, VNCSessionMetadata>();
        public static Dictionary<string,VncInteractionResponse> VNCSessionResponse = new Dictionary<string, VncInteractionResponse>();
        public static Dictionary<string, VncDisplay> VNCUIComponents = new Dictionary<string, VncDisplay>();

        public static async Task InitOrUpdateSession(VNCSessionMetadata meta, VncInteractionResponse response)
        {
            if(ImplantVNCSessionMeta.ContainsKey(meta.SessionID))
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

        public static async Task UpdateDisplaySession(string sessionId)
        {
            if (VNCUIComponents.ContainsKey(sessionId))
            {
               await VNCUIComponents[sessionId].HandleUpdate(VNCSessionResponse[sessionId], ImplantVNCSessionMeta[sessionId]);
            }
        }

    }
}
