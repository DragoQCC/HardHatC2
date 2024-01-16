using System.Net.WebSockets;

namespace HardHatCore.ContractorSystem.Services.Communication
{
    public class HH_WebSocketComms
    {
        private readonly ContractSystemFactory _coreFactory;

        public HH_WebSocketComms(ContractSystemFactory coreFactory)
        {
            _coreFactory = coreFactory;
        }

        public static async Task HandleWebSocket(WebSocket webSocket)
        {
            // WebSocket communication handling
            string receivedData;
           
        }

    }
}
