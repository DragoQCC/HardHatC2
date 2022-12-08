using Donut;
using Donut.Structs;
using System;
using System.IO;

namespace TeamServer.Utilities
{
    public static class Shellcode
    {
        public static byte[] AssemToShellcode(string filePath, string arguments)
        {
            // remove the leading space and add " to front and end of arguments
            arguments = arguments.TrimStart(' ');
            filePath = filePath.TrimStart(' ');
          
            DonutConfig config = new DonutConfig();
            config.Arch = 3;
            config.Bypass = 3;
            config.InputFile = @$"{filePath}";
            config.Args = arguments;
            config.Payload = "D:\\My_Custom_Code\\HardHatC2\\TeamServer\\temp\\shellcode.bin";
            int ret = Generator.Donut_Create(ref config);
            Console.WriteLine(Helper.GetError(ret));
            byte[] shellcode = File.ReadAllBytes($"{config.Payload}");
            return shellcode;
        }
    }
}
