using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class Delete : EngineerCommand
    {
        public override string Name => "delete";

        public override string Execute(EngineerTask task)
        {
            if (!task.Arguments.TryGetValue("/file", out string file))
            {
                return "error: " + "no file to delete set pls use the /file key";
            }
            //delete file
            File.Delete(file);
            return $"Deleted {file}";
        }
    }
}
