using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Models
{

    [Serializable]
    public class C2Message
    {
        public List<string> PathMessage { get; set; }       
        public byte[] Data { get; set; }
        public int MessageType { get; set; } // 1 = Tasking, 2 = Notification, 3-100 = Reserved for future use, 100+ = Plugin specific
    }
}
