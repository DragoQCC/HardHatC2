namespace ApiModels.Shared
{
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
}
