using ApiModels.Shared;
using SQLite;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TeamServer.Models;
using TeamServer.Utilities;
//using DynamicEngLoading;
namespace TeamServer.Models.Dbstorage
{
    [Table("EngineerTaskResult")]
    public class EngineerTaskResult_DAO
    {
        //, AutoIncrement]
        //public int Id { get; set; }

        [PrimaryKey]
        [Column("TaskId")]
        public string TaskId { get; set; } //Task ID

        [Column("Command")]
        public string Command { get; set; } //Command that was run

        [Column("Result")]
        public byte[] Result { get; set; }

        [Column("IsHidden")]
        public bool IsHidden { get; set; }

        [Column("EngineerId")]
        public string EngineerId { get; set; }

        [Column("Status")]
        public EngTaskStatus Status { get; set; }

        [Column("ResultType")]
        public TaskResponseType ResponseType { get; set; }

        [Column("SeenTaskList")]
        public byte[] UsersThatHaveReadResult { get; set; } //list of usernames that have read this result for client ui tracking of notifications

        //create an implcit operator to convert from model to dao
        public static implicit operator EngineerTaskResult_DAO(EngineerTaskResult model)
        {
            return new EngineerTaskResult_DAO
            {
                TaskId = model.Id,
                Command = model.Command,
                Result = model.Result,
                IsHidden = model.IsHidden,
                EngineerId = model.EngineerId,
                Status = (EngTaskStatus)model.Status,
                ResponseType = (TaskResponseType)model.ResponseType,
                UsersThatHaveReadResult = model.UsersThatHaveReadResult.Serialize()
            };
        }
       
        //create an implcit operator to convert from dao to model
        public static implicit operator EngineerTaskResult(EngineerTaskResult_DAO dao)
        {
            return new EngineerTaskResult
            {
                Id = dao.TaskId,
                Command = dao.Command,
                Result = dao.Result,
                IsHidden = dao.IsHidden,
                EngineerId = dao.EngineerId,
                Status = (EngTaskStatus)dao.Status,
                ResponseType = (TaskResponseType)dao.ResponseType,
                UsersThatHaveReadResult = dao.UsersThatHaveReadResult.Deserialize<List<string>>()
            };
        }
    }
}
