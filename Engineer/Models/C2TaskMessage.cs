using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Models
{

    [Serializable]
    public class C2TaskMessage
    {
        public List<string> PathMessage { get; set; }
        
        public byte[] TaskData { get; set; }
    }
}
