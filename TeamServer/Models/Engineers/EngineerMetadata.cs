using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using NetSerializer;


// amde to keep Engineer clean just holds list of proerties every Engineer will have.
namespace TeamServer.Models
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
