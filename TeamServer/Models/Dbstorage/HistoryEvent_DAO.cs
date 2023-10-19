using System;
using HardHatCore.TeamServer.Models.Extras;
using SQLite;

namespace HardHatCore.TeamServer.Models.Dbstorage
{
    [Table("HistoryEvent")]
    public class HistoryEvent_DAO
    {
        [PrimaryKey]
        [Column("Id")]
        public string Id { get; set; }

        [Column("Event")]
        public string Event { get; set; }

        [Column("Time")]
        public string Time { get; set; }

        [Column("Status")]
        public string Status { get; set; }

        //create a implicit operator to convert from the model to the DAO
        public static implicit operator HistoryEvent_DAO(HistoryEvent model)
        {
            return new HistoryEvent_DAO
            {
                Id = model.Id,
                Event = model.Event,
                Time = model.Time,
                Status = model.Status
            };
        }

        //create a implicit operator to convert from the DAO to the model
        public static implicit operator HistoryEvent(HistoryEvent_DAO dao)
        {
            return new HistoryEvent
            {
                Id = dao.Id,
                Event = dao.Event,
                Time = dao.Time,
                Status = dao.Status
            };
        }
    }






}
