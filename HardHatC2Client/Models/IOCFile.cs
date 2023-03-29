namespace HardHatC2Client.Models;

public class IOCFile
{
    public string ID { get; set; }
    public string  Name { get; set; }
    public string UploadedHost { get; set; }
    public string UploadedPath { get; set; }
    public DateTime Uploadtime { get; set; }
    public string  md5Hash { get; set; }
}