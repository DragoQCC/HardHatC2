using SQLite;

namespace TeamServer.Models.Database
{
    [Table("UserRoles")]
    public class UserRoleInfo
    {
        //unique guid for table 
        [PrimaryKey]
        [Column("UserRoleId")]
        public string UserRoleID { get; set; }
        //id of the user
        [Column("UserId")]
        public string UserID { get; set; }
        //id of the role
        [Column("RoleId")]
        public string RoleID { get; set; }
        //human readable name of role
        [Column("RoleName")]
        public string RoleName { get; set; }
    }
}
