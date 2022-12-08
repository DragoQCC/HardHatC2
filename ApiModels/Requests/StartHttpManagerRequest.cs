using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
	public class StartHttpmanagerRequest
	{
		public string Name { get; set; }
		public int ConnectionPort { get; set; }
        public string ConnectionAddress { get; set; }
        public bool IsSecure { get; set; }
		public C2Profile C2profile { get; set; }
        public ManagerType managertype { get; set; }


        public enum ManagerType
        {
            http, https, tcp, smb
        }
    }
}
