using HardHatC2Client.Pages;

namespace HardHatC2Client.Models
{
    public class InteractiveTerminalCommand
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string TabId { get; set; }
        public string Command { get; set; }
        public string Output { get; set; }
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        public string Originator { get; set; } = Login.SignedInUser;

    }
}
