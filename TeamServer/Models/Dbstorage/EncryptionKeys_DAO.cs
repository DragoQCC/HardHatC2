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
        
        //normally Engineer id or keyword for teamserver keys
        [Column("ItemID")]
        public string ItemID { get; set; }

        
    }
}
