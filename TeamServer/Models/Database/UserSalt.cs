using SQLite;

namespace TeamServer.Models.Database
{
    [Table("UsersSalt")]
    public class UserSalt
    {
        //unique guid for table
        [PrimaryKey]
        [Column("UserSaltId")]
        public string Id { get; set;}
        //id of the user
        [Column("UserId")]
        public string UserId { get; set; }
        //salt for the user
        [Column("Salt")]
        public byte[] Salt { get; set; }

    }
}
