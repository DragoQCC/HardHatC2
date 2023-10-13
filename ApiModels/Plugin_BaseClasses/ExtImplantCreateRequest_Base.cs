using ApiModels.Plugin_Interfaces;
using ApiModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Plugin_BaseClasses
{
    public class ExtImplantCreateRequest_Base : IExtImplantCreateRequest
    {
        public DateTime? selectedKillDate { get; set; }
        public TimeSpan? selectedKillTime { get; set; }
        public string managerName { get; set; }
        public int ConnectionAttempts { get; set; }
        public int Sleep { get; set; }
        public string? WorkingHours { get; set; }
        public DateTime? KillDateTime { get; set; }
        public bool? EncodeShellcode { get; set; }
        public ImpCompileType complieType { get; set; }
        public SleepTypes SleepType { get; set; } = SleepTypes.None;
        //expected to come from the implant plugin
        public string implantType { get; set; }
        public List<string>? IncludedCommands { get; set; }
        public List<string>? IncludedModules { get; set; }
        public bool? IsPostEx { get; set; }
        public bool? IsChunkEnabled { get; set; }
        public int? ChunkSize { get; set; }
    }
}
