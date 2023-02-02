using System;
using System.Collections.Generic;

namespace TeamServer.Models.Extras
{
    public class DownloadFile
    {
        public static List<DownloadFile> downloadFiles = new();

        public string Name { get; set; }

        public string Host { get; set; }
        public string OrginalPath { get; set; }

        public string SavedPath { get; set; }

        public DateTime downloadedTime = DateTime.UtcNow;
    }
}
