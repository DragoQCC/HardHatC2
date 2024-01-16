using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Shared;

namespace HardHatCore.ApiModels.Plugin_BaseClasses
{
    public class ExtImplantCreateRequest_Base : IExtImplantCreateRequest , ICloneable
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
        public List<SerializedExtras>? Extras { get; set; } = new(); //can be used for extra build options that is not part of the base class

        public object Clone()
        {
            return new ExtImplantCreateRequest_Base()
            {
                selectedKillDate = this.selectedKillDate,
                selectedKillTime = this.selectedKillTime,
                managerName = this.managerName,
                ConnectionAttempts = this.ConnectionAttempts,
                Sleep = this.Sleep,
                WorkingHours = this.WorkingHours,
                EncodeShellcode = this.EncodeShellcode,
                complieType = this.complieType,
                SleepType = this.SleepType,
                implantOsType = this.implantOsType,
                implantType = this.implantType,
                KillDateTime = this.KillDateTime,
                IncludedCommands = this.IncludedCommands,
                IncludedModules = this.IncludedModules,
                ChunkSize = this.ChunkSize,
                IsChunkEnabled = this.IsChunkEnabled,
                Extras = this.Extras
            };
        }
    }
}
