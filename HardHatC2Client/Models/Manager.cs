using ApiModels.Shared;

namespace HardHatC2Client.Models
{
    public class Manager
	{
		public string? Name { get; set; }
		public string? ConnectionAddress { get; set; }
		public int? ConnectionPort { get; set; }
		public string? NamedPipe { get; set; } = "";
		public bool? Active { get; set; }
		public ManagerType Type { get; set; } // enum of values http,https,tcp,smb
		public DateTime? CreationTime { get; set; }
		
		public string? BindAddress { get; set; } //address local to the teamserver to bind to
        public int? BindPort { get; set; } //port a tcp client connects to or the port an http manager listens on
        public int? ListenPort { get; set; } //port a tcp server listens on
        public bool? IsLocalHost { get; set; } //sets if the server should listen on only localhost or on 0.0.0.0

        public C2Profile C2profile { get; set; }

        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child
	}
}
