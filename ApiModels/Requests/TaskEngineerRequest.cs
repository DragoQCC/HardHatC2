using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
	public class TaskEngineerRequest
	{
		public string? Command { get; set; }
        public Dictionary<string, string>? Arguments { get; set; } // this way arguments can have a name like /arg value then /arg1 value1 etc. 
        public byte[]? File { get; set; }
        public string? taskID { get; set; }
		public bool IsBlocking { get; set; }
	}
}
