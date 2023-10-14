using System;
using ApiModels.Plugin_BaseClasses;
using SQLite;
using TeamServer.Plugin_BaseClasses;
using TeamServer.Utilities;

namespace TeamServer.Models.Dbstorage
{
    [Table("ExtImplant")]
    public class ExtImplant_DAO
    {
        [PrimaryKey]
        public string id { get; set; }

        [Column("number")]
        public int number { get; set; }

        [Column("note")]
        public string note { get; set; }

        [Column("Metadata")]
        public byte[] Metadata { get; set; }

        [Column("ConnectionType")]
        public string ConnectionType { get; set; }

        [Column("ExternalAddress")]
        public string ExternalAddress { get; set; }

        [Column("LastSeen")]
        public DateTime LastSeen { get; set; }

        [Column("FirstSeen")]
        public DateTime FirstSeen { get; set; }

        [Column("ImplantType")]
        public string ImplantType { get; set; }

        [Column("Status")]
        public string Status { get; set; }

        //create an implicit operator to convert from the model to the DAO
        public static implicit operator ExtImplant_DAO(ExtImplant_Base model)
        {
            return new ExtImplant_DAO
            {

                id = model.Metadata.Id,
                number = model.Number,
                note = model.Note,
                Metadata = model.Metadata.Serialize(),
                ConnectionType = model.ConnectionType,
                ExternalAddress = model.ExternalAddress,
                LastSeen = model.LastSeen,
                FirstSeen = model.FirstSeen,
                ImplantType = model.ImplantType,
                Status = model.Status
            };
        }

        //create an implicit operator to convert from the DAO to the model
        public static implicit operator ExtImplant_Base(ExtImplant_DAO dao)
        {
            return new ExtImplant_Base
            {
                Metadata = dao.Metadata.Deserialize<ExtImplantMetadata_Base>(),
                Number = dao.number,
                Note = dao.note,
                ConnectionType = dao.ConnectionType,
                ExternalAddress = dao.ExternalAddress,
                LastSeen = dao.LastSeen,
                FirstSeen = dao.FirstSeen,
                ImplantType = dao.ImplantType,
                Status = dao.Status,
            };
        }

    }
}
