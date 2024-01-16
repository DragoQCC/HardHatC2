using System.Collections.Generic;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.TeamServer.Utilities;
using SQLite;
//using DynamicEngLoading;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("ExtImplantTask")]
    public class ExtImplantTask_DAO
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Column("Command")]
        public string Command { get; set; }

        [Column("Arguments")]
        public byte[] Arguments { get; set; }

        [Column("TaskHeader")]
        public string TaskHeader { get; set; }

        [Column("RequiresPreProc")]
        public bool RequiresPreProc { get; set; }

        [Column("RequiresPostProc")]
        public bool RequiresPostProc { get; set; }
        [Column("IssuingUser")]
        public string IssuingUser { get; set; }
        [Column("ImplantId")]
        public string ImplantId { get; set; }

        //create an implicit operator to convert from the model to the doa
        public static implicit operator ExtImplantTask_DAO(ExtImplantTask_Base model)
        {
            return new ExtImplantTask_DAO
            {
                Id = model.Id,
                Command = model.Command,
                Arguments = model.Arguments.Serialize(),
                TaskHeader = model.TaskHeader,
                RequiresPreProc = model.RequiresPreProc,
                RequiresPostProc = model.RequiresPostProc,
                IssuingUser = model.IssuingUser,
                ImplantId = model.ImplantId
            };
        }


        //create an implicit operator to convert from the doa to the model
        public static implicit operator ExtImplantTask_Base(ExtImplantTask_DAO dao)
        {
            return new ExtImplantTask_Base
            {
                Id = dao.Id,
                Command = dao.Command,
                Arguments = dao.Arguments.Deserialize<Dictionary<string, string>>(),
                TaskHeader = dao.TaskHeader,
                RequiresPreProc = dao.RequiresPreProc,
                RequiresPostProc = dao.RequiresPostProc,
                IssuingUser = dao.IssuingUser,
                ImplantId = dao.ImplantId
            };
        }
    }
}
