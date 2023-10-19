using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ApiModels.Plugin_Interfaces
{
    public interface IExtImplantTask
    {
        public string Id { get; set; }

        public string Command { get; set; }

        public Dictionary<string, string> Arguments { get; set; }

        public byte[] File { get; set; }

        public bool IsBlocking { get; set; }
    }
}
