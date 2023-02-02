using SQLite;
using TeamServer.Models.Engineers;

namespace TeamServer.Models.Dbstorage
{

    [Table("EncryptionKeys")]
    public class EncryptionKeys_DAO
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Column("Key")]
        public string Key { get; set; }
        [Column("EngineerID")]
        public string EngineerID { get; set; }


        ////create a implicit operator to convert from the DAO to the Model
        //public static implicit operator EncryptionKeys(EncryptionKeys_DAO dao)
        //{
        //    return new EncryptionKeys
        //    {
        //        Id = dao.Id,
        //        Key = dao.Key,
        //        EngineerID = dao.EngineerID
        //    };
        //}

        ////create a implicit operator to convert from the Model to the DAO
        //public static implicit operator EncryptionKeys_DAO(EncryptionKeys model)
        //{
        //    return new EncryptionKeys_DAO
        //    {
        //        Id = model.Id,
        //        Key = model.Key,
        //        EngineerID = model.EngineerID
        //    };
        //}
    }
}
