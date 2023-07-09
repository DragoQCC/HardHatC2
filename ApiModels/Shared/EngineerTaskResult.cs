using System.Collections.Generic;

namespace ApiModels.Shared
{
    public class EngineerTaskResult
    {
        public List<string> UsersThatHaveReadResult { get; set; } = new(); //key is username, value is a list of ids of tasks that have been seen

        public string Id { get; set; }

        public string Command { get; set; } //Command that was run

        public string EngineerId { get; set; }

        public byte[] Result { get; set; }

        public object ResultObject { get; set; }

        public bool IsHidden { get; set; }

        public EngTaskStatus Status { get; set; }
        public TaskResponseType ResponseType { get; set; }
    }
}
