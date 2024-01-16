using System;
using System.Collections.Generic;
using HardHatCore.ApiModels.Shared;


namespace HardHatCore.ApiModels.Plugin_Interfaces
{
    public interface IExtImplantCreateRequest
    {
        public DateTime? selectedKillDate { get; set; }
        public TimeSpan? selectedKillTime { get; set; }
        public string managerName { get; set; }
        public int? ConnectionAttempts { get; set; }
        public int Sleep { get; set; }
        public string? WorkingHours { get; set; }
        public DateTime? KillDateTime { get; set; }
        public bool? EncodeShellcode { get; set; }
        public ImpCompileType complieType { get; set; }
        public SleepTypes SleepType { get; set; }
        //expected to come from the implant plugin
        public string implantType { get; set; }
        public string implantOsType { get; set; }
        public List<string>? IncludedCommands { get; set; }
        public List<string>? IncludedModules { get; set; }
        public bool? IsPostEx { get; set; }
        public bool? IsChunkEnabled { get; set; }
        public int? ChunkSize { get; set; }
        public List<SerializedExtras>? Extras { get; set; } //can be used for extra build options that is not part of the base class, if a stock UI element is needed set the ClientUIType property to the desired UI element type
    }
}
