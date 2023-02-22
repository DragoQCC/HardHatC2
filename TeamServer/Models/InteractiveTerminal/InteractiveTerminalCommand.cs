using System;
using System.Collections.Generic;

namespace TeamServer.Models.InteractiveTerminal
{
    //these should only ever be created from the client and are filled here for propigation and backup
    public class InteractiveTerminalCommand
    {
        public static List<InteractiveTerminalCommand> TerminalCommands = new List<InteractiveTerminalCommand>();

        public string Id { get; set; }
        public string TabId { get; set; }
        public string Command { get; set; }
        public string Output { get; set; }
        public string Timestamp { get; set; } 
        public string Originator { get; set; }
    }
}
