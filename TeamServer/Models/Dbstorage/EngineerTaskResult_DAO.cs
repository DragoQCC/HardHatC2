using SQLite;
using System.Runtime.CompilerServices;
using TeamServer.Models;
namespace TeamServer.Models.Dbstorage
{
    [Table("EngineerTaskResult")]
    public class EngineerTaskResult_DAO
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

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
                ResponseType = (TaskResponseType)model.ResponseType
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
                Status = (Models.EngTaskStatus)dao.Status,
                ResponseType = (Models.TaskResponseType)dao.ResponseType
            };
        }

    }
    public enum EngTaskStatus
    {
        Running = 2,
        Complete = 3,
        FailedWithWarnings = 4,
        CompleteWithErrors = 5,
        Failed = 6,
        Cancelled = 7
    }

    public enum TaskResponseType
    {
        None,
        String,
        FileSystemItem,
        ProcessItem,
    }
}
