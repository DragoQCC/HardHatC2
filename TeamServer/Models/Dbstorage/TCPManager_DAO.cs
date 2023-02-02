using SQLite;
using System;
using System.Threading;
using TeamServer.Models.Managers;

namespace TeamServer.Models.Dbstorage
{
    [Table("TCPManagers")]
    public class TCPManager_DAO
    {
        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public int Id { get; set; }
        [Column("Name")]
        public string Name { get; set;}
        [Column("ConnectionAddress")]
        public string ConnectionAddress { get; set; }
        [Column("BindPort")]
        public int BindPort { get; set; } //port a tcp client connects to 
        [Column("ListenPort")]
        public int ListenPort { get; set; } //port a tcp server listens on
        [Column("IsLocalHost")]
        public bool IsLocalHost { get; set; } //sets if the server should listen on only localhost or on 0.0.0.0

        [Column("ConnectionMode")]
        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child


        
        public enum ConnectionMode
        {
            bind,
            reverse
        }

        //make a static implcit operator to convert from an actual object to a DAO object
        public static implicit operator TCPManager_DAO(TCPManager manager)
        {
            return new TCPManager_DAO
            {
                Name = manager.Name,
                ConnectionAddress = manager.ConnectionAddress,
                BindPort = manager.BindPort,
                ListenPort = manager.ListenPort,
                IsLocalHost = manager.IsLocalHost,
                connectionMode = (ConnectionMode)manager.connectionMode
            };
        }

        //make a static implcit operator to convert from a DAO object to an actual object
        public static implicit operator TCPManager(TCPManager_DAO manager)
        {
            return new TCPManager
            {
                Name = manager.Name,
                ConnectionAddress = manager.ConnectionAddress,
                BindPort = manager.BindPort,
                ListenPort = manager.ListenPort,
                IsLocalHost = manager.IsLocalHost,
                connectionMode = (TCPManager.ConnectionMode)manager.connectionMode
            };
        }



    }
}
