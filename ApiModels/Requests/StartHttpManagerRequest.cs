using ApiModels.Shared;

namespace ApiModels.Requests
{
	public class StartHttpmanagerRequest
	{
		public string Name { get; set; }
		public int ConnectionPort { get; set; }
        public string ConnectionAddress { get; set; }
        public string BindAddress { get; set; }
        public int BindPort { get; set; }
        public bool IsSecure { get; set; }
		public C2Profile C2profile { get; set; }
        public ManagerType managertype { get; set; }
    }
}
