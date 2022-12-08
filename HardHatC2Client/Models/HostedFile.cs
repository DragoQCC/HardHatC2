namespace HardHatC2Client.Models;

public class HostedFile
{
    public string Name { get; set; }
    public DateTime LastWriteTime { get; set; } = DateTime.UtcNow;

    public string size { get; set; }
}