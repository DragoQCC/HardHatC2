using System.Collections.Generic;

namespace TeamServer.Models.Engineers.TaskResultTypes
{
    public class FilePart
    {
        //key is the task id and the value is the concatenated file parts 
        //once the file is fully downloaded and saved on disk its entry will be removed from this dictionary
        public static Dictionary<string, byte[]> FinalFileTracking = new();

        public int Type { get; set; } // 1 = file part, 2 = end of file
        public int Length { get; set; } // Size of data in bytes
        public byte[] Data { get; set; } // Actual data
    }
}
