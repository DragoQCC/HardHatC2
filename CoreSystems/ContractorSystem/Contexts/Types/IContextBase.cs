using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ContractorSystem.Contexts.Types
{
    public interface IContextBase
    {
        //this is the unique id of the context
        public string Id { get; set; }

        //should match the name of one of the invocation method, property, or task
        // ex.OnManagerCreated
        public string Name { get; set; }

        //this is the description of the context
        //should provide useful information about the context such as what it does, when its invoked, etc
        public string Description { get; set; }

        //this is the default object(s) that will be passed to the callback
        //ex. OnManagerCreated_End will pass the manager object to the callback
        public Dictionary<string, object> Properties { get; set; }

        //this is the type of the object that will be passed to the callback can be filled so the receiver knows what type of object to expect
        //ex. Event, DataChange, Task, etc
        public string _contextType { get; set; }
    }
}
