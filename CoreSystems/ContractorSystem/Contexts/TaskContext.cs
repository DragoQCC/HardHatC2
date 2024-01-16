using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ContractorSystem.Contexts
{
    internal class TaskContext : IContextBase
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ContractId { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public Dictionary<string, object> ExtraProperties { get; set; }
        public ContextType _contextType { get; set; }
        public ContextReceiverRole ContextReceiver { get; set; }
    }
}
