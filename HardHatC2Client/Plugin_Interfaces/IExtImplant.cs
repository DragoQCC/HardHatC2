using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;

namespace HardHatCore.HardHatC2Client.Plugin_Interfaces
{
    public interface IExtImplant
    {
        public ExtImplantMetadata_Base Metadata { get; set; }
        public string ImplantType { get; set; }
        public int Number { get; set; }
        public string Note { get; set; }
        public string ConnectionType { get; set; }
        public string ExternalAddress { get; set; }
        public DateTime LastSeen { get; set; }
        public string LastSeenTimer { get; set; }
        public DateTime FirstSeen { get; set; }
        public string Status { get; set; }
    }
}
