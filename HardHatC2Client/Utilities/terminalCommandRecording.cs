using System.Diagnostics;
using System.Text;

namespace HardHatC2Client.Utilities
{
    public class terminalCommandRecording
    {
        public static string TerminalCommandExecute(string argument)
        {
            
            //check the current operating system and make sure its linux
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Console.WriteLine("Linux OS detected starting terminal");
                //create a new process
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = "-c \"" + argument + "\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = false
                    }
                };

                //create a new string builder
                var output = new StringBuilder();

                //add the output to the string builder
                process.OutputDataReceived += (_, args) => { output.AppendLine(args.Data); };
                process.ErrorDataReceived += (_, args) => { output.AppendLine(args.Data); };

                //start the process
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                //return the output
                return output.ToString();
            }
            else
            {
                //return an error if the operating system is not linux
                return "error: " + "This command is only supported on linux";
            }          
        }
    }
}
