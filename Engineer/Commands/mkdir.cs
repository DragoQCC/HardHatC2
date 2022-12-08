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
    internal class mkdir : EngineerCommand
    {
        public override string Name => "mkdir";

        public override string Execute(EngineerTask task)
        {
            task.Arguments.TryGetValue("/path", out string path);
            if (Directory.Exists(path))
            {
                return "error: " + "Directory already exists";
            }
            //try to create directory 
            try
            {
                Directory.CreateDirectory(path);
                return "Directory created";
            }
            catch (Exception e)
            {
                return "error: " + e.Message;
            }

        }
    }
}
