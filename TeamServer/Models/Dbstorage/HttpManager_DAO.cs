using System.Security.Cryptography.X509Certificates;
using System.Threading;
using ApiModels.Requests;
using SQLite;
using TeamServer.Utilities;

namespace TeamServer.Models.Dbstorage
{
    [Table("HttpManager")]
    public class HttpManager_DAO
    {
        [Column("Id"), PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [Column("Name")]
        public string Name { get; set; } // properties allows us to get Name when manager is created later so set will go with creation functions later
        
        [Column("ConnectionPort")]
        public int ConnectionPort { get; set; }         // bind port for http manager again set on creation 
        
        [Column("ConnectionAddress")]
        public string ConnectionAddress { get; set; }   // bind address for http manager again set on creation
        
        [Column("IsSecure")]
        public bool IsSecure { get; set; }
        
        [Column("CertificatePath")]
        public string CertificatePath { get; set; }

        [Column("C2Profile")]
        public byte[] c2Profile { get; set; }

        //create a implicit operator to convert from HttpManager_DAO to HttpManager
        public static implicit operator Httpmanager(HttpManager_DAO dao)
        {
            return new Httpmanager
            {
                Name = dao.Name,
                ConnectionPort = dao.ConnectionPort,
                ConnectionAddress = dao.ConnectionAddress,
                IsSecure = dao.IsSecure,
                CertificatePath = dao.CertificatePath,
                c2Profile = dao.c2Profile.ProDeserializeForDatabase<C2Profile>()
            };
        }

        //create a implicit operator to convert from HttpManager to HttpManager_DAO
        public static implicit operator HttpManager_DAO(Httpmanager manager)
        {
            return new HttpManager_DAO
            {
                Name = manager.Name,
                ConnectionPort = manager.ConnectionPort,
                ConnectionAddress = manager.ConnectionAddress,
                IsSecure = manager.IsSecure,
                CertificatePath = manager.CertificatePath,
                c2Profile = manager.c2Profile.ProSerialiseForDatabase()
            };
        }

    }
}
