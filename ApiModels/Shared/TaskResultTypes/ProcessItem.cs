namespace ApiModels.Shared.TaskResultTypes
{
    public class ProcessItem
    {
        public string ProcessName { get; set; }
        public string ProcessPath { get; set; }
        public string Owner { get; set; }
        public int ProcessId { get; set; }
        public int ProcessParentId { get; set; }
        public int SessionId { get; set; }
        public string Arch { get; set; }
    }
}
