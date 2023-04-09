using ApiModels.Responses;
using HardHatC2Client.Pages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace HardHatC2Client.Models
{
    public class Engineer
    {
        public EngineerMetadata engineerMetadata { get; set;}       
        
        public int Number { get; set; }
        public string Id { get; set; }
        public string Address { get; set; }
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string Integrity { get; set; }
        public string Arch { get; set; }
        public DateTime LastSeen { get; set; }
        public string LastSeenTimer { get; set; }

        public DateTime FirstSeen { get; set; }

        public string ExternalAddress { get; set; }
        public string ConnectionType { get; set; }
        public string ManagerName { get; set; }

        public int Sleep { get; set; }
        public string Status { get; set; }

        public void Init()
        {
            Id = engineerMetadata.Id;
            Address = engineerMetadata.Address;
            Hostname = engineerMetadata.Hostname;
            Username = engineerMetadata.Username;
            ProcessName = engineerMetadata.ProcessName;
            ProcessId = engineerMetadata.ProcessId;
            Integrity = engineerMetadata.Integrity;
            Arch = engineerMetadata.Arch;
            ManagerName = engineerMetadata.ManagerName;
            Sleep = engineerMetadata.Sleep;
        }
    }
}
