namespace HardHatC2Client.Models
{
    public class EngineerTaskResult
    {
        public List<string> UsersThatHaveReadResult { get; set; } //key is username, value is a list of ids of tasks that have been seen
        
        public string Id { get; set; }

        public string Command { get; set; } //Command that was run

        public string EngineerId { get; set; }

        public byte[] Result { get; set; }

        public object ResultObject { get; set; }

        public EngTaskStatus status { get; set; }
        public TaskResponseType ResponseType { get; set; }

        
        public enum EngTaskStatus
        {
            Pending = 0,
            Tasked = 1,
            Running = 2,
            Complete = 3,
            FailedWithWarnings = 4,
            CompleteWithErrors = 5,
            Failed = 6,
            Cancelled = 7,
            NONE
        }

        public enum TaskResponseType
        {
            None,
            String,
            FileSystemItem,
            ProcessItem,
            TokenStoreItem,
            HelpMenuItem
        }
    }
}
