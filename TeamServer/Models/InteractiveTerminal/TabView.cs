using System.Collections.Generic;

namespace TeamServer.Models.InteractiveTerminal
{
    public class TabView
    {
        public static List<TabView> Tabs = new List<TabView>();

        public string Name { get; set; }
        public List<InteractiveTerminalCommand> Content { get; set; }
        public string TabId { get; set; }
    }
}
