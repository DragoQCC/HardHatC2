using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;

namespace HardHatCore.ContractorSystem.Contexts.Types
{
    public class EventContext : IContextBase
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Name { get; init; }
        public string? Description { get; set; }
        public List<byte[]>? Data { get; set; }
        public Dictionary<string, string>? DataTags { get; set; }
        public string ContextType { get; init; } = "Event";
        public string SourceLocation { get; init; }
        
        [JsonIgnore]
        internal List<IContractor>? Subscribers { get; set; } = new List<IContractor>();

        public EventContext(string name, string desc, string sourceloc)
        {             
            Name = name;
            Description = desc;
            SourceLocation = sourceloc;
        }
    }
}
