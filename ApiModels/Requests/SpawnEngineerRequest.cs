using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
    public class SpawnEngineerRequest
    {
        public DateTime? selectedKillDate { get; set; }
        public TimeSpan? selectedKillTime { get; set; }

        public string managerName { get; set; }
        public int ConnectionAttempts { get; set; }
        public int Sleep { get; set; }
        public string? WorkingHours { get; set; }

        public DateTime? KillDateTime { get; set; }

        public bool EncodeShellcode { get; set; }

        public EngCompileType complieType { get; set; }

        public SleepTypes SleepType { get; set; }

        public ImplantType implantType { get; set; } = ImplantType.Engineer; // default to engineer
        
        public List<string> IncludedCommands { get; set; } = new List<string>();

        public List<string> IncludedModules { get; set; } = new List<string>();

        public bool IsPostEx { get; set; } = false;

        public enum EngCompileType
        {
            exe, 
            shellcode, 
            powershellcmd, 
            dll, 
            serviceexe
        }

        public enum SleepTypes
        {
            None,
            Custom_RC4,
            //Ekko,
            //Foliage,
        }
        
        public enum ImplantType
        {
            Engineer,
            Constructor,
            Rustineer,
        }
    }
}
