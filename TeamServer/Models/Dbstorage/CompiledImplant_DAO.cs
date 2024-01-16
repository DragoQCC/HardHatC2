using System.Collections.Generic;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Utilities;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("CompiledImplant")]
    public class CompiledImplant_DAO
    {
        [PrimaryKey]
        public string Id { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("ImplantType")]
        public string ImplantType { get; set; }
        [Column("CompileDateTime")]
        public string CompileDateTime { get; set; }
        [Column("IncludedCommands")]
        public byte[] IncludedCommands { get; set; } 
        [Column("IncludedModules")]
        public byte[] IncludedModules { get; set; }
        [Column("CompileType")]
        public string CompileType { get; set; }
        [Column("SleepType")]
        public string SleepType { get; set; }
        [Column("Sleep")]
        public string Sleep { get; set; }
        [Column("ConnectionAttempts")]
        public string ConnectionAttempts { get; set; }
        [Column("WorkingHours")]
        public string WorkingHours { get; set; }
        [Column("KillDateTime")]
        public string KillDateTime { get; set; }
        [Column("ManagerName")]
        public string ManagerName { get; set; }
        [Column("SavedPath")]
        public string SavedPath { get; set; }


        //create an implict operator to convert from the model to the DAO
        public static implicit operator CompiledImplant_DAO(CompiledImplant model)
        {
            return new CompiledImplant_DAO
            {
                Id = model.Id,
                Name = model.Name,
                ImplantType = model.ImplantType,
                CompileDateTime = model.CompileDateTime,
                IncludedCommands = model.IncludedCommands.Serialize(),
                IncludedModules = model.IncludedModules.Serialize(),
                CompileType = model.CompileType,
                SleepType = model.SleepType,
                Sleep = model.Sleep,
                ConnectionAttempts = model.ConnectionAttempts,
                WorkingHours = model.WorkingHours,
                KillDateTime = model.KillDateTime,
                ManagerName = model.ManagerName,
                SavedPath = model.SavedPath
            };
        }

        //create an implict operator to convert from the DAO to the model
        public static implicit operator CompiledImplant(CompiledImplant_DAO dao)
        {
            return new CompiledImplant
            {
                Id = dao.Id,
                Name = dao.Name,
                ImplantType = dao.ImplantType,
                CompileDateTime = dao.CompileDateTime,
                IncludedCommands = dao.IncludedCommands.Deserialize<List<string>>(),
                IncludedModules = dao.IncludedModules.Deserialize<List<string>>(),
                CompileType = dao.CompileType,
                SleepType = dao.SleepType,
                Sleep = dao.Sleep,
                ConnectionAttempts = dao.ConnectionAttempts,
                WorkingHours = dao.WorkingHours,
                KillDateTime = dao.KillDateTime,
                ManagerName = dao.ManagerName,
                SavedPath = dao.SavedPath
            };
        }
    }
}
