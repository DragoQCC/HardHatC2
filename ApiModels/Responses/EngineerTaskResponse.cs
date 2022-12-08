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
		public TaskStatus status { get; set; }
		public enum TaskStatus
		{
		Pending,
		Tasked,
		Running,
		Complete,
		Cancelled,
		Aborted
		}
	}
}
