using System;
using SQLite;
using TeamServer.Utilities;

namespace TeamServer.Models.Dbstorage
{
    [Table("Engineer")]
    public class Engineer_DAO
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("engineerMetadata")]
        public byte[] engineerMetadata { get; set; }

        [Column("ConnectionType")]
        public string ConnectionType { get; set; }

        [Column("ManagerName")]
        public string ManagerName { get; set; }

        [Column("ExternalAddress")]
        public string ExternalAddress { get; set; }

        [Column("LastSeen")]
        public DateTime LastSeen { get; set; }

        [Column("Status")]
        public string Status { get; set; }

        [Column("Sleep")]
        public int Sleep { get; set; }

        //create an implicit operator to convert from the model to the DAO
        public static implicit operator Engineer_DAO(Engineer model)
        {
            return new Engineer_DAO
            {
                engineerMetadata = model.engineerMetadata.ProSerialise(),
                ConnectionType = model.ConnectionType,
                ManagerName = model.ManagerName,
                ExternalAddress = model.ExternalAddress,
                LastSeen = model.LastSeen,
                Status = model.Status,
                Sleep = model.Sleep
            };
        }

        //create an implicit operator to convert from the DAO to the model
        public static implicit operator Engineer(Engineer_DAO dao)
        {
            return new Engineer
            {
                engineerMetadata = dao.engineerMetadata.ProDeserialize<EngineerMetadata>(),
                ConnectionType = dao.ConnectionType,
                ManagerName = dao.ManagerName,
                ExternalAddress = dao.ExternalAddress,
                LastSeen = dao.LastSeen,
                Status = dao.Status,
                Sleep = dao.Sleep
            };
        }

    }
}
