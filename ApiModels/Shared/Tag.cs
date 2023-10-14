using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Shared
{
    public class Tag
    {
        public static List<Tag> Existing_Tags { get; set; } = new();

        public static Dictionary<string, List<string>> Task_Tags { get; set; } = new(); // key is the tag name, and value is the list of task ids with that tag set
        public static Dictionary<string, List<string>> Terminal_Tags { get; set; } = new(); // key is the tag name, and value is the list of task ids with that tag set

        public string Name { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
    }
}
