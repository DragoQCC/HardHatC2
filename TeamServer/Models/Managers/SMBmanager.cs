using System.Threading;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Shared;

namespace HardHatCore.TeamServer.Models
{
    public class SMBManager : Manager
    {
        public override string Name { get;  set; }
        public string NamedPipe { get; set; }
        public string ConnectionAddress { get; set; }
        public bool Active => _tokenSource is not null && !_tokenSource.IsCancellationRequested;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        public override ManagerType Type { get; set; } = ManagerType.smb;

        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child

        public SMBManager(string name, string namedPipe)
        {
            Name = name;
            NamedPipe = namedPipe;
            connectionMode = ConnectionMode.bind;
        }
        public SMBManager(string name, string namedPipe, string connectionAddress) // then then engineer being made is a client and needs the ip to know where the named pipe is hosted in the network 
        {
            Name = name;
            NamedPipe = namedPipe;
            ConnectionAddress = connectionAddress;
            connectionMode = ConnectionMode.reverse;
        }

        public SMBManager() { }

        public override Task Start()
        {
            //return an empty task obect
            return Task.CompletedTask;
        }

        public override void Stop()
        {
            _tokenSource.Cancel();
        }
    }
}
