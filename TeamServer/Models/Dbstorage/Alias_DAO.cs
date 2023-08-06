using SQLite;
using TeamServer.Models.Extras;

namespace TeamServer.Models.Dbstorage
{
    [Table("Alias")]
    public class Alias_DAO
    {
        [PrimaryKey]
        [Column("id")]
        public string id { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Value")]
        public string Value { get; set; }
        [Column("Username")]
        public string Username { get; set; }


        public static implicit operator Alias_DAO(Alias alias)
        {
            return new Alias_DAO
            {
                id = alias.id,
                Name = alias.Name,
                Value = alias.Value,
                Username = alias.Username

            };
        }

        public static implicit operator Alias(Alias_DAO alias)
        {
            return new Alias
            {
                id = alias.id,
                Name = alias.Name,
                Value = alias.Value,
                Username = alias.Username
            };
        }
    }
}
