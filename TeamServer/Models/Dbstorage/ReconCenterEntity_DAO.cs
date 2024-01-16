using System.Collections.Generic;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Utilities;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("ReconCenterEntity")]
    public class ReconCenterEntity_DAO
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
       
        [Column("Name")]
        public string Name { get; set; }
       
        [Column("Description")]
        public string Description { get; set; }

        [Column("Properties")]
        public byte[] Properties { get; set; }


        //make a implicit conversion from the model to the dao
        public static implicit operator ReconCenterEntity_DAO(ReconCenterEntity model)
        {
            return new ReconCenterEntity_DAO
            {
                Name = model.Name,
                Description = model.Description,
                Properties = model.Properties.ProSerialiseForDatabase()
            };
        }

        //make a implicit conversion from the dao to the model
        public static implicit operator ReconCenterEntity(ReconCenterEntity_DAO dao)
        {
            return new ReconCenterEntity
            {
                Name = dao.Name,
                Description = dao.Description,
                Properties = dao.Properties.ProDeserializeForDatabase<List<ReconCenterEntity.ReconCenterEntityProperty>>()
            };
        }
    }
}
