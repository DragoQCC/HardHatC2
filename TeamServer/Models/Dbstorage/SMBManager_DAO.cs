using HardHatCore.ApiModels.Shared;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("SMBManager")]
    public class SMBManager_DAO
    {
        [PrimaryKey,AutoIncrement]
        public int Id { get; set; }
        
        [Column("Name")]
        public  string Name { get; set; }
        
        [Column("NamedPipe")]
        public string NamedPipe { get; set; }
       
        [Column("ConnectionAddress")]
        public string ConnectionAddress { get; set; }
        
        [Column("ConnectionMode")]
        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child

        //make a static implicit operator to convert from the Manager object to the db object
        public static implicit operator SMBManager_DAO(SMBManager manager)
        {
            return new SMBManager_DAO
            {
                Name = manager.Name,
                NamedPipe = manager.NamedPipe,
                ConnectionAddress = manager.ConnectionAddress,
                connectionMode = (ConnectionMode)manager.connectionMode
            };
        }

        //make a static implicit operator to convert from the db object to the Manager object
        public static implicit operator SMBManager(SMBManager_DAO dao)
        {
            return new SMBManager
            {
                Name = dao.Name,
                NamedPipe = dao.NamedPipe,
                ConnectionAddress = dao.ConnectionAddress,
                connectionMode = (ConnectionMode)dao.connectionMode
            };
        }
    }
}
