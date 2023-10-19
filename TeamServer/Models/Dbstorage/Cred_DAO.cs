using System;
using HardHatCore.TeamServer.Models.Extras;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("Cred")]
    public class Cred_DAO
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }

        [Column("Domain")]
        public string Domain { get; set; }

        [Column("CredentialValue")]
        public string CredentialValue { get; set; }

        [Column("Type")]
        public CredType Type { get; set; }

        [Column("SubType")]
        public string SubType { get; set; }

        [Column("CaptureTime")]
        public DateTime CaptureTime { get; set; } 
        public enum CredType
        {
            hash, password, ticket
        }

        //create an implict operator to convert from the model to the DAO
        public static implicit operator Cred_DAO(Cred model)
        {
            return new Cred_DAO
            {
                Name = model.Name,
                Domain = model.Domain,
                CredentialValue = model.CredentialValue,
                Type = (CredType)model.Type,
                SubType = model.SubType,
                CaptureTime = model.CaptureTime
            };
        }

        //create an implict operator to convert from the DAO to the model
        public static implicit operator Cred(Cred_DAO dao)
        {
            return new Cred
            {
                Name = dao.Name,
                Domain = dao.Domain,
                CredentialValue = dao.CredentialValue,
                Type = (Cred.CredType)dao.Type,
                SubType = dao.SubType,
                CaptureTime = dao.CaptureTime
            };
        }

    }
}
