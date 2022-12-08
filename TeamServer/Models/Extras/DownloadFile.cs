using System;

namespace TeamServer.Models.Extras
{
    public class DownloadFile
    {
        public string Name { get; set; }
        public string OrginalPath { get; set; }

        public string SavedPath { get; set; }

        public DateTime downloadedTime = DateTime.UtcNow;
    }
}
