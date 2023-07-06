namespace HardHatC2Client.Models
{
    public class CompiledImplant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ImplantType { get; set; }
        public string CompileDateTime { get; set; } 
        public List<string> IncludedCommands { get; set; } = new List<string>();
        public List<string> IncludedModules { get; set; } = new List<string>();
        public string CompileType { get; set; }
        public string SleepType { get; set; }
        public string Sleep { get; set; }
        public string ConnectionAttempts { get; set; }
        public string WorkingHours { get; set; }
        public string KillDateTime { get; set; }
        public string ManagerName { get; set; }
        public string  SavedPath { get; set; }
    }
}
