using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;

namespace HardHatCore.ContractorSystem.Contexts.Types
{
    public class EventContext : IContextBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
        public string _contextType { get; set; } = "Event";
        public IEnumerable<IContractor>? EventSubs { get; set; }
    }
}
