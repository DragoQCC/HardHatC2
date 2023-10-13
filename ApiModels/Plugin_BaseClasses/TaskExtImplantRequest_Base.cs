using ApiModels.Plugin_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Plugin_BaseClasses
{
    public class TaskExtImplantRequest_Base : ITaskExtImplantRequest
    {
        public string? Command { get; set; }
        public Dictionary<string, string>? Arguments { get; set; }
        public byte[]? File { get; set; }
        public string? taskID { get; set; }
        public bool IsBlocking { get; set; }
        public bool RequiresPreProc { get; set; }
        public bool RequiresPostProc { get; set; }
        public string? TaskHeader { get; set; }
        public string IssuingUser { get; set; } = "";

        public Dictionary<string, byte[]>? TaskingExtras { get; set; } = new();
    }
}
