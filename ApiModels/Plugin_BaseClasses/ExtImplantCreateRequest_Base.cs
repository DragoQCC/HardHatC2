using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Shared;

namespace HardHatCore.ApiModels.Plugin_BaseClasses
{
    public class ExtImplantCreateRequest_Base : IExtImplantCreateRequest
    {
        public DateTime? selectedKillDate { get; set; }
        public TimeSpan? selectedKillTime { get; set; }
        public string managerName { get; set; }
        public int? ConnectionAttempts { get; set; }
        public int Sleep { get; set; }
        public string? WorkingHours { get; set; }
        public DateTime? KillDateTime { get; set; }
        public bool? EncodeShellcode { get; set; } = false;
        public ImpCompileType complieType { get; set; }
        public SleepTypes SleepType { get; set; } = SleepTypes.None;
        public string implantOsType { get; set; }

        //expected to come from the implant plugin
        public string implantType { get; set; }
        public List<string>? IncludedCommands { get; set; } = new();
        public List<string>? IncludedModules { get; set; } = new();
        public bool? IsPostEx { get; set; } = false;
        public bool? IsChunkEnabled { get; set; } = false;
        public int? ChunkSize { get; set; } = 0;
    }
}
