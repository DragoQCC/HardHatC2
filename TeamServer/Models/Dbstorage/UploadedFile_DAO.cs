using SQLite;
using TeamServer.Models.Extras;
namespace TeamServer.Models.Dbstorage
{
    [Table("UploadedFile")]
    public class UploadedFile_DAO
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
       
        [Column("Name")]
        public string Name { get; set; }

        [Column("SavedPath")]
        public string SavedPath { get; set; }
        [Column("FileContent")]
        public byte[] FileContent { get; set; }


        //create an implcit operator to convert from the model to the DAO
        public static implicit operator UploadedFile_DAO(UploadedFile model)
        {
            return new UploadedFile_DAO
            {
                Name = model.Name,
                SavedPath = model.SavedPath,
                FileContent = model.FileContent
            };
        }

        //create an implcit operator to convert from the DAO to the model
        public static implicit operator UploadedFile(UploadedFile_DAO dao)
        {
            return new UploadedFile
            {
                Name = dao.Name,
                SavedPath = dao.SavedPath,
                FileContent = dao.FileContent
            };
        }
    }
}
