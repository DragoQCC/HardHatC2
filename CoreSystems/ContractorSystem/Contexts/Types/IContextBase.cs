using System.ComponentModel.DataAnnotations;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;


namespace HardHatCore.ContractorSystem.Contexts.Types
{
    public interface IContextBase
    {
        //this is the unique id of the context
        [Required]
        public string Id { get; init; }

        //should match the name of one of the invocation method, property, or task
        // ex.OnManagerCreated
        [Required]
        public string Name { get; init; }

        //this is the description of the context, Optional but should provide useful information about the context such as what it does, when its invoked, etc
        public string? Description { get; set; }

        //this is the data that will be passed to the reviver of the context
        //ex. OnManagerCreated will pass the properties used to generate a manager such as Id, Ip, Port, Urls, etc. 
        //ex. OnManagerCreated_End will pass a serialized manager object
        public List<byte[]>? Data { get; set; }

        // this is the og list of type and property names that go with the data
        // key is the PropertyName and value is the Type
        public Dictionary<string, string>? DataTags { get; set; } 

        //this is the type of the object that will be passed to the callback can be filled so the receiver knows what type of object to expect
        //ex. Event, DataChange, Task, etc.
        [Required]
        public string ContextType { get; init; }

        //this is the location of the source of the context
        //ex. TeamServer, HHClient, etc.
        [Required]
        public string SourceLocation { get; init; }
    }
}
