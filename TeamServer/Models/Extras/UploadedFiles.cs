using System.Collections.Generic;

namespace HardHatCore.TeamServer.Models.Extras
{
    public class UploadedFile
    {
        public static  List<UploadedFile> uploadedFileList = new();
        public string Name { get; set; }
        public string SavedPath { get; set; }  
        public byte[] FileContent { get; set;}
    }
}
