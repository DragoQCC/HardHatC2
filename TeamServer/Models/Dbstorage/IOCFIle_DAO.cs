using System;
using HardHatCore.TeamServer.Models.Extras;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage;

[Table("IOCFIle")]
public class IOCFIle_DAO
{
    [PrimaryKey]
    public string ID { get; set; }
    [Column("Name")]
    public string  Name { get; set; }
    [Column("UploadedHost")]
    public string UploadedHost { get; set; }
    [Column("UploadedPath")]
    public string UploadedPath { get; set; }
    [Column("Uploadtime")]
    public DateTime Uploadtime { get; set; }
    [Column("md5Hash")]
    public string  md5Hash { get; set; }
    
    
    //create an implcit operator to convert from the model to the DAO
    public static implicit operator IOCFIle_DAO(IOCFile model)
    {
        return new IOCFIle_DAO
        {
            ID = model.ID,
            Name = model.Name,
            UploadedHost = model.UploadedHost,
            UploadedPath = model.UploadedPath,
            Uploadtime = model.Uploadtime,
            md5Hash = model.md5Hash
        };
    }
    //create an implcit operator to convert from the DAO to the model
    public static implicit operator IOCFile(IOCFIle_DAO dao)
    {
        return new IOCFile
        {
            ID = dao.ID,
            Name = dao.Name,
            UploadedHost = dao.UploadedHost,
            UploadedPath = dao.UploadedPath,
            Uploadtime = dao.Uploadtime,
            md5Hash = dao.md5Hash
        };
    }
}