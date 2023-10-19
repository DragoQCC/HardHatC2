using System;
using System.Collections.Generic;

namespace HardHatCore.TeamServer.Models.Extras
{
    public class Alias
    {
        [NonSerialized]
        public static List<Alias> savedAliases = new();

        public string id { get; set; } 
        public string Name { get; set; }
        public string Value { get; set; }
        public string Username { get; set; }
    }
}
