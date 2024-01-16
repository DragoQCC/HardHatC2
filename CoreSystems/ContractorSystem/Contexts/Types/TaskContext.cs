using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ContractorSystem.Contexts.Types
{
    public class TaskContext : IContextBase
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string Name { get; init; }
        public string? Description { get; set; }
        public List<byte[]>? Data { get; set; }
        public Dictionary<string, string>? DataTags { get; set; }

        //Specific to tasks, this is the data that will be returned to the caller
        public List<byte[]>? ReturnData { get; set; }
        public Dictionary<string, string>? ReturnDataTags { get; set; }
        public string ContextType { get; init; } = "Task";
        // the location the task will be sent to, for example a task to send the logging will be sent to the teamServer
        public string SourceLocation { get; init; }
        
        //the location the task was sent from, for example a external python Contractor might want to get the logs from the teamServer
        public string TaskingLocation { get; set; }

        public TaskContext(string name, string desc, string sourceloc)
        {
            Name = name;
            Description = desc;
            SourceLocation = sourceloc;
        }
    }
}
