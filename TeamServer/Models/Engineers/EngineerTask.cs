using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace TeamServer.Models
{
    [Serializable]
    public class EngineerTask
    {
        
      
        public string Id { get; set; }
     
        public string Command { get; set; }
    
        public Dictionary<string, string> Arguments { get; set; }
     
        public byte[] File { get; set; }
      
        public bool IsBlocking { get; set; }

        public EngineerTask() { }

        public EngineerTask(string Id, string Command, Dictionary<string, string> Arguments, byte[] File, bool IsBlocking)
        {
            this.Id = Id;
            this.Command = Command;
            this.Arguments = Arguments;
            this.File = File;
            this.IsBlocking = IsBlocking;
        }


    }
}
