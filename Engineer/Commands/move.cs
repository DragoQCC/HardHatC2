using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Engineer.Commands
{
    internal class move : EngineerCommand
    {
        public override string Name => "move";

        public override string Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/dest", out string destination);
            task.Arguments.TryGetValue("/file", out string file);

            //move file to destination location
            if (File.Exists(file))
            {
                File.Move(file, destination);
                return "File moved";
            }
            return "error: " + "file not found";

        }
    }
}
