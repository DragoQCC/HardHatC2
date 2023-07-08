using ApiModels.Shared;
using System;

namespace ApiModels.Responses
{
    public class ManagerResponse
    {
		public string Name { get; set; }
		public string ConnectionAddress { get; set; }
		public int ConnectionPort { get; set; }
		public bool Active { get; set; }
		public ManagerType Type { get; set; } // enum of values http,https,tcp,smb
        public DateTime CreationTime { get; set; }
	}
}
