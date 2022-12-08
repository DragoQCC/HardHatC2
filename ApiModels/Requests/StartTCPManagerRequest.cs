using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
    public class StartTCPManagerRequest
    {
        public string Name { get; set; }
        public string ConnectionAddress { get; set; } // address a tcp client connects to, enabled on reverse
        public int BindPort { get; set; } //port a tcp client connects to , enabled on reverse 
        public int ListenPort { get; set; } //port a tcp server listens on , enabled on bind
        public bool IsLocalHost { get; set; } //sets if the server should listen on only localhost or on 0.0.0.0 , enabled on bind
        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child

        public ManagerType managertype { get; set; }

        public enum ConnectionMode
        {
            bind,
            reverse
        }
        public enum ManagerType
        {
            http, https, tcp, smb
        }
    }
}
