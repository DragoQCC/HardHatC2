using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_Interfaces;

namespace HardHatCore.ApiModels.Plugin_BaseClasses
{
    [Serializable]
    public class ExtImplantTask_Base : IExtImplantTask
    {
        public ExtImplantTask_Base(string taskID, string command, Dictionary<string, string> arguments, byte[] file, bool isBlocking, bool req_preProc, bool req_PostProc, string Taskheader, string issuingUser, string implantId)
        {
            Id = taskID;
            Command = command;
            Arguments = arguments;
            File = file;
            IsBlocking = isBlocking;
            RequiresPreProc = req_preProc;
            RequiresPostProc = req_PostProc;
            TaskHeader = Taskheader;
            IssuingUser = issuingUser;
            ImplantId = implantId;
        }

        public ExtImplantTask_Base() { }

        public string Id { get; set; }
        public string Command { get; set; }
        public Dictionary<string, string> Arguments { get; set; }
        public byte[] File { get; set; }
        public bool IsBlocking { get; set; }
        
        //these values here are for the server to use not the implant
        public bool RequiresPreProc { get; set; } = false;
        public bool RequiresPostProc { get; set; } = false;
        public string TaskHeader { get; set; }    
        public string IssuingUser { get; set; }
        public string ImplantId { get; set; }


        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ExtImplantTask_Base);
        }
        public bool Equals(ExtImplantTask_Base obj)
        {
            return obj != null && obj.Id == this.Id;
        }

    }
}
