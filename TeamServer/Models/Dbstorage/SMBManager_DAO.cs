using SQLite;
using HardHatCore.ApiModels.Shared;

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

        //make a static implicit operator to convert from the manager object to the db object
        public static implicit operator SMBManager_DAO(SMBmanager manager)
        {
            return new SMBManager_DAO
            {
                Name = manager.Name,
                NamedPipe = manager.NamedPipe,
                ConnectionAddress = manager.ConnectionAddress,
                connectionMode = (ConnectionMode)manager.connectionMode
            };
        }

        //make a static implicit operator to convert from the db object to the manager object
        public static implicit operator SMBmanager(SMBManager_DAO dao)
        {
            return new SMBmanager
            {
                Name = dao.Name,
                NamedPipe = dao.NamedPipe,
                ConnectionAddress = dao.ConnectionAddress,
                connectionMode = (ConnectionMode)dao.connectionMode
            };
        }
    }
}
