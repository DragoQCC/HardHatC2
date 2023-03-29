using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
    public class SpawnEngineerRequest
    {
        public string managerName { get; set; }
        public int ConnectionAttempts { get; set; }
        public int Sleep { get; set; }
        public string? WorkingHours { get; set; }
        
        public bool EncodeShellcode { get; set; }

        public EngCompileType complieType { get; set; }

        public SleepTypes SleepType { get; set; }

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
            // Ekko,
        }
    }
}
