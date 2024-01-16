using System.Threading;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Shared;

namespace HardHatCore.TeamServer.Models.Managers
{
    public class TCPManager : Manager
    {
        public override string Name { get; set; }
        public string ConnectionAddress { get; set; }
        public int BindPort { get; set; } //port a tcp client connects to 
        public int ListenPort { get; set; } //port a tcp server listens on
        public bool IsLocalHost { get; set; } //sets if the server should listen on only localhost or on 0.0.0.0

        public bool Active => _tokenSource is not null && !_tokenSource.IsCancellationRequested;
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();
        public override ManagerType Type { get; set; } = ManagerType.tcp;
        public ConnectionMode connectionMode { get; set; } // always means direction of parent -> child
        

        public TCPManager(string name, int listenPort, bool isLocalHost ) //bind 
        {
            Name = name;
            ListenPort = listenPort;
            IsLocalHost = isLocalHost;
            connectionMode = ConnectionMode.bind;
        }
        public TCPManager(string name,string connectionAddress, int bindPort ) //reverse
        {
            Name = name;
            ConnectionAddress = connectionAddress;
            BindPort = bindPort;
            connectionMode = ConnectionMode.reverse;
        }

        public TCPManager()
        { }

        public override Task Start()
        {
            //return an empty task obect
            return Task.CompletedTask;
        }

        public override void Stop()
        {
            
        }
    }
}
