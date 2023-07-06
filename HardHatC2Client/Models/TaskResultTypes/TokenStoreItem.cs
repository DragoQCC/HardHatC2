namespace HardHatC2Client.Models.TaskResultTypes
{
    public class TokenStoreItem
    {
        public int Index { get; set; }
        public string Username { get; set; }
        public int PID { get; set; }
        public string SID { get; set; }
        public bool IsCurrent { get; set; }
    }
}
