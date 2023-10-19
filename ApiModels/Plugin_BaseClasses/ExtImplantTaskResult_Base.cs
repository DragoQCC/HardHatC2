using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_Interfaces;

namespace HardHatCore.ApiModels.Plugin_BaseClasses
{
    public class ExtImplantTaskResult_Base : IExtImplantTaskResult
    {
        public List<string> UsersThatHaveReadResult { get; set; } = new();
        public string Id { get; set; }
        public string Command { get; set; }
        public string ImplantId { get; set; }
        public byte[] Result { get; set; }
        public object ResultObject { get; set; }
        public bool IsHidden { get; set; }
        public ExtImplantTaskStatus Status { get; set; }
        public ExtImplantTaskResponseType ResponseType { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ExtImplantTaskResult_Base);
        }
        public bool Equals(ExtImplantTaskResult_Base obj)
        {
            return obj != null && obj.Id == this.Id;
        }
    }
}
