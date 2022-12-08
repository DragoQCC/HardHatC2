using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Models
{
    [Serializable]
    public class EngineerMetadata
    {
		public string Id { get; set; }
		public string Hostname { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
		public string ProcessName { get; set; }
		public int ProcessId { get; set; }
        public string Integrity { get; set; }
        public string Arch { get; set; }
        public string ManagerName { get; set; }
        public int Sleep { get; set; }        
    }
}
