using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ApiModels.Shared
{
    [Serializable]
    public class C2Message
    {
        public List<string> PathMessage { get; set; } // outbound this is the path to the implant, inbound this contains the id and name of the implant in the first 2 elements
        public byte[] Data { get; set; }
        public int MessageType { get; set; } // 1 = Tasking, 2 = Notification, 3-100 = Reserved for future use, 100+ = Plugin specific
    }

}
