using System;
using HardHatCore.TeamServer.Models.Extras;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("DownloadFile")]
    public class DownloadFile_DAO
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Host")]
        public string Host { get; set; }

        [Column("OrginalPath")]
        public string OrginalPath { get; set; }

        [Column("SavedPath")]
        public string SavedPath { get; set; }

        [Column("downloadedTime")]
        public DateTime downloadedTime { get; set; }

        //create an implcit operator to convert from the model to the DAO
        public static implicit operator DownloadFile_DAO(DownloadFile model)
        {
            return new DownloadFile_DAO
            {
                Name = model.Name,
                Host = model.Host,
                OrginalPath = model.OrginalPath,
                SavedPath = model.SavedPath,
                downloadedTime = model.downloadedTime
            };
        }

        //create an implcit operator to convert from the DAO to the model
        public static implicit operator DownloadFile(DownloadFile_DAO dao)
        {
            return new DownloadFile
            {
                Name = dao.Name,
                Host = dao.Host,
                OrginalPath = dao.OrginalPath,
                SavedPath = dao.SavedPath,
                downloadedTime = dao.downloadedTime
            };
        }


    }

}
