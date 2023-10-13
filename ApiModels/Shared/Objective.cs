using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Shared
{
    public class Objective
    {
        public static List<Objective> Existing_Objectives { get; set; } = new();
        public static Dictionary<Objective, HashSet<string>> Objectives_FinishedSubTasks { get; set; } = new();

        public string Name { get; set; }
        public string Description { get; set; }
        public ObjectiveType Type { get; set; }
        public ObjectiveStatus Status { get; set; }
        public Tag Tag_Value { get; set; } = new();
        public List<string> SubTasks { get; set; } = new();

        public string _subtaskString = "";

        public enum ObjectiveType
        {
            Primary,
            Secondary,
            Tertiary
        }

        public enum ObjectiveStatus
        {
            NotStarted,
            InProgress,
            Incomplete,
            Missed,
            PartiallyComplete,
            Complete
        }
    }

}
