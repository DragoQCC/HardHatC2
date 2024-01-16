﻿using System.Collections.Generic;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.TeamServer.Utilities;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("ExtImplantTaskResult")]
    public class ExtImplantTaskResult_DAO
    {
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
        public string ImplantId { get; set; }

        [Column("Status")]
        public ExtImplantTaskStatus Status { get; set; }

        [Column("ResultType")]
        public ExtImplantTaskResponseType ResponseType { get; set; }

        [Column("SeenTaskList")]
        public byte[] UsersThatHaveReadResult { get; set; } //list of usernames that have read this result for client ui tracking of notifications

        //create an implcit operator to convert from model to dao
        public static implicit operator ExtImplantTaskResult_DAO(ExtImplantTaskResult_Base model)
        {
            if (model is null)
            {
                return null;
            }
            return new ExtImplantTaskResult_DAO
            {
                TaskId = model.Id,
                Command = model.Command,
                Result = model.Result,
                IsHidden = model.IsHidden,
                ImplantId = model.ImplantId,
                Status = model.Status,
                ResponseType = model.ResponseType,
                UsersThatHaveReadResult = model.UsersThatHaveReadResult.Serialize()
            };
        }
       
        //create an implcit operator to convert from dao to model
        public static implicit operator ExtImplantTaskResult_Base(ExtImplantTaskResult_DAO dao)
        {
            if (dao is null)
            {
                return null;
            }
            return new ExtImplantTaskResult_Base
            {
                Id = dao.TaskId,
                Command = dao.Command,
                Result = dao.Result,
                IsHidden = dao.IsHidden,
                ImplantId = dao.ImplantId,
                Status = dao.Status,
                ResponseType = dao.ResponseType,
                UsersThatHaveReadResult = dao.UsersThatHaveReadResult.Deserialize<List<string>>()
            };
        }
    }
}
