using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Utilities;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("HttpManager")]
    public class HttpManager_DAO
    {
        [Column("Id"), PrimaryKey]
        public string Id { get; set; }
        
        [Column("Name")]
        public string Name { get; set; } // properties allows us to get Name when Manager is created later so set will go with creation functions later
        
        [Column("ConnectionPort")]
        public int ConnectionPort { get; set; }         // bind port for http Manager again set on creation 
        
        [Column("ConnectionAddress")]
        public string ConnectionAddress { get; set; }   // bind address for http Manager again set on creation
        
        [Column("BindPort")]
        public int BindPort { get; set; }         // bind port for http Manager again set on creation 
        
        [Column("BindAddress")]
        public string BindAddress { get; set; }   // bind address for http Manager again set on creation
        
        [Column("IsSecure")]
        public bool IsSecure { get; set; }
        
        [Column("CertificatePath")]
        public string CertificatePath { get; set; }

        [Column("C2Profile")]
        public byte[] c2Profile { get; set; }

        //create a implicit operator to convert from HttpManager_DAO to HttpManager
        public static implicit operator HttpManager(HttpManager_DAO dao)
        {
            //var httpman = HttpManager.HttpManagerFactoryFunc(dao.Name,dao.ConnectionAddress,dao.ConnectionPort,dao.BindAddress,dao.BindPort,dao.IsSecure,dao.c2Profile.ProDeserializeForDatabase<C2Profile>());
            //httpman.Id = dao.Id;
            //return httpman;
            return new HttpManager()
            {
                Id = dao.Id,
                Name = dao.Name,
                ConnectionPort = dao.ConnectionPort,
                ConnectionAddress = dao.ConnectionAddress,
                BindPort = dao.BindPort,
                BindAddress = dao.BindAddress,
                IsSecure = dao.IsSecure,
                CertificatePath = dao.CertificatePath,
                c2Profile = dao.c2Profile.ProDeserializeForDatabase<C2Profile>()
            };
        }

        //create a implicit operator to convert from HttpManager to HttpManager_DAO
        public static implicit operator HttpManager_DAO(HttpManager manager)
        {
            return new HttpManager_DAO
            {
                Id = manager.Id,
                Name = manager.Name,
                ConnectionPort = manager.ConnectionPort,
                ConnectionAddress = manager.ConnectionAddress,
                BindPort = manager.BindPort,
                BindAddress = manager.BindAddress,
                IsSecure = manager.IsSecure,
                CertificatePath = manager.CertificatePath,
                c2Profile = manager.c2Profile.ProSerialiseForDatabase()
            };
        }

    }
}
