﻿namespace HardHatCore.HardHatC2Client.Models.TaskResultTypes;

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
        
        public List<ACL> ACLs { get; set; } = new List<ACL>();
}

[Serializable]
public class ACL
{
        public string IdentityRef { get; set; } = "";
        public string AccessControlType { get; set; } = "";
        public string FileSystemRights { get; set; } = "";
        public bool IsInherited { get; set; }
}