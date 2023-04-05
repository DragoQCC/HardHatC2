namespace HardHatC2Client.Models
{
    public class EngineerTaskResult
    {
        public string Id { get; set; }

        public object Result { get; set; }

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
            HelpMenuItem
        }
    }
}
