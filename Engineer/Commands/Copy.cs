using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Engineer.Models;

namespace Engineer.Commands
{
    internal class Copy : EngineerCommand
    {
        public override string Name => "copy";

        public override string Execute(EngineerTask task)
        {
            if (!task.Arguments.TryGetValue("/file", out string file))
            {
                return "error: " + "no file to copy set pls use the /file key";
            }
            if (!task.Arguments.TryGetValue("/dest", out string destination))
            {
                return "error: " + "no destination file set pls use the /destination key";
            }
            //copy file to destionation
            File.Copy(file, destination);
            return $"Copied {file} to {destination}";

        }
    }
}
