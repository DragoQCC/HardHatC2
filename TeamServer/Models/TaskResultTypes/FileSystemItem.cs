using System;

namespace HardHatCore.TeamServer.Models.TaskResultTypes;

[Serializable]
public class FileSystemItem
{

    public string Name { get; set; } = "";
    public long Length { get; set; } = 0;
    public string Owner { get; set; } = "";
    public long ChildItemCount { get; set; } = 0;
    public DateTime CreationTimeUtc { get; set; } = new DateTime();
    public DateTime LastAccessTimeUtc { get; set; } = new DateTime();
    public DateTime LastWriteTimeUtc { get; set; } = new DateTime();
    public string DirACL { get; set; } = " ";
    public string FileACL { get; set; } = " ";

}