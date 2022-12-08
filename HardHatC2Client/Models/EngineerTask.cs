namespace HardHatC2Client.Models
{
    public class EngineerTask
    {
        public string Id { get; set; }
        public string Command { get; set; }
        public Dictionary<string,string> Arguments { get; set; }
        public byte[] File { get; set; }
    }
}
