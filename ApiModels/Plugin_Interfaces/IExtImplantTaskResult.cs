using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Plugin_Interfaces
{
    public interface IExtImplantTaskResult
    {
        public List<string> UsersThatHaveReadResult { get; set; } //key is username, value is a list of ids of tasks that have been seen
        public string Id { get; set; }

        public string Command { get; set; } //Command that was run

        public string ImplantId { get; set; }

        public byte[] Result { get; set; }

        public object ResultObject { get; set; }

        public bool IsHidden { get; set; }

        public ExtImplantTaskStatus Status { get; set; }
        public ExtImplantTaskResponseType ResponseType { get; set; }
    }
}
