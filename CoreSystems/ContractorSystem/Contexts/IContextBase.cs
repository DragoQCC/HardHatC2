using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ContractorSystem.Contexts
{
    public interface IContextBase
    {
        public string Id { get; set; }

        public string ContractId { get; set; }

        //should match the name of one of the invocation methods
        // ex.OnManagerCreated
        public string Name { get; set; }

        //this is the default object(s) that will be passed to the callback
        //ex. OnManagerCreated_End will pass the manager object to the callback
        public Dictionary<string, object> Properties { get; set; }
        
        // extra items to pass to the callback, typically defined by the contract members
        public Dictionary<string, object> ExtraProperties { get; set; }

        //this is the type of context this is
        public ContextType _contextType { get; set; }

        //this is the intended receiver of this context
        public ContextReceiverRole ContextReceiver { get;  set; }
    }

    public enum ContextType
    {
        Event = 0,
        Task = 1,
        DataChange = 2,
    }

    public enum ContextReceiverRole
    {
        ContractOriginator,
        ContractSigner,
    }
}
