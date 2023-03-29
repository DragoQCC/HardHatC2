using System;
using System.Collections.Generic;

namespace TeamServer.Models.Extras;

public class IOCFile
{
    public static List<IOCFile> IOCFiles { get; set; }
    
    public static Dictionary<string,IOCFile>  PendingIOCFiles = new Dictionary<string,IOCFile>();

    public string ID { get; set; }
    public string  Name { get; set; }
    public string UploadedHost { get; set; }
    public string UploadedPath { get; set; }
    public DateTime Uploadtime { get; set; }
    public string  md5Hash { get; set; }
}