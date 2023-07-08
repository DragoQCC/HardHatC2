using SQLite;

namespace TeamServer.Models.Database
{
    [Table("Roles")]
    public class RoleInfo
    {
        //unique guid for table
        [PrimaryKey]
        [Column("RoleId")]
        public string Id { get; set; }
        //human readable name of role
        [Column("RoleName")]
        public string Name { get; set; }
    }
}
