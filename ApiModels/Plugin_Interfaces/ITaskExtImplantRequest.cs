using System.Collections.Generic;


namespace ApiModels.Plugin_Interfaces
{
    public interface ITaskExtImplantRequest
    {
        public string? Command { get; set; }
        // this way arguments can have a structure like /arg value then /arg1 value1 etc.
        public Dictionary<string, string>? Arguments { get; set; }
        public byte[]? File { get; set; }
        public string? taskID { get; set; }
        public bool IsBlocking { get; set; }
        // string is the name of the extra, object is the seralized value of the extra, not intened to be used by the implant but extra info for the team server
        public Dictionary<string, byte[]>? TaskingExtras { get; set; }
    }
}
