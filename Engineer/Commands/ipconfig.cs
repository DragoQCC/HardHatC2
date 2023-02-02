using Engineer.Commands;
using Engineer.Functions;
using Engineer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Commands
{
    internal class ipconfig : EngineerCommand
    {
        public override string Name => "ipconfig";

        public override async Task Execute(EngineerTask task)
        {
            //get all of the ip addresses and subnet masks
            var ipAddresses = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(x => x.Address.ToString() + " " + x.IPv4Mask.ToString())
                .ToList();
            //use a string builder to build the output one address and mask per line
            var output = new StringBuilder();
            foreach (var ipAddress in ipAddresses)
            {
                output.AppendLine(ipAddress);
            }
            Tasking.FillTaskResults(output.ToString(),task,EngTaskStatus.Complete);
        }
    }
}
