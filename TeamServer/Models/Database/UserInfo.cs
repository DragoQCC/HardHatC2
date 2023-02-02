using Microsoft.AspNetCore.Identity;
using SQLite;

namespace TeamServer.Models.Database
{
    [Table("Users")]
    public class UserInfo
    {
        //unique guid for table
        [PrimaryKey]
        [Column("UserId")]
        public string Id { get; set;}
        //username
        [Column("UserName")]
        public string UserName { get; set;}
        // normalizedusername
        [Column("NormalizedUserName")]
        public string NormalizedUserName { get; set; }
        //password
        [Column("PasswordHash")]
        public string PasswordHash { get; set;}

        
    }
}
