using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Responses
{
    public class EngineerTaskResponse
    {
		public string Id { get; set; }
		public string Result { get; set; }
		public EngTaskStatus status { get; set; }
		public enum EngTaskStatus
		{
		    Pending = 0 ,
		    Tasked = 1,
            Running = 2,
            Complete = 3,
            FailedWithWarnings = 4,
            CompleteWithErrors = 5,
            Failed = 6,
            Cancelled = 7
        }
	}
}
