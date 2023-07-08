using ApiModels.Shared;

namespace ApiModels.Requests
{
    public class StartManagerRequest
    {
        public string Name { get; set; }
        public int ConnectionPort { get; set; }
        public string? ConnectionAddress { get; set; }
        public string? BindAddress { get; set; }
        public bool IsSecure { get; set; }
        public string? NamedPipe { get; set; }
        public C2Profile? C2profile { get; set; }
        public int BindPort { get; set; } //port a tcp client connects to , enabled on reverse 
        public int ListenPort { get; set; } //port a tcp server listens on , enabled on bind
        public bool IsLocalHost { get; set; } //sets if the server should listen on only localhost or on 0.0.0.0 , enabled on bind
        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child
        public ManagerType managertype { get; set;}
    }
}
