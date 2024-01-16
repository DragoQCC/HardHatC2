using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_Interfaces;

namespace HardHatCore.ApiModels.Plugin_BaseClasses
{
    public class TaskExtImplantRequest_Base : ITaskExtImplantRequest
    {
        public string? Command { get; set; }
        public Dictionary<string, string>? Arguments { get; set; } = new();
        public byte[]? File { get; set; }
        public bool IsBlocking { get; set; } = false;
        public bool RequiresPreProc { get; set; } = false;
        public bool RequiresPostProc { get; set; } = false;
        public Dictionary<string, byte[]>? TaskingExtras { get; set; } = new();



        public TaskExtImplantRequest_Base(string command, Dictionary<string, string>? arguments, byte[]? file, bool isBlocking, bool requiresPreProc, bool requiresPostProc, Dictionary<string, byte[]>? taskingExtras)
        {
            Command = command;
            Arguments = arguments;
            File = file;
            IsBlocking = isBlocking;
            RequiresPreProc = requiresPreProc;
            RequiresPostProc = requiresPostProc;
            TaskingExtras = taskingExtras;
        }
    }
}
