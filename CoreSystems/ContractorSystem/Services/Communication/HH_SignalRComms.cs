using HardHatCore.ContractorSystem.Contexts.Types;
using Microsoft.AspNetCore.SignalR;

namespace HardHatCore.ContractorSystem.Services.Communication
{
    public partial class HH_SignalRComms : Hub
    {
        private readonly ContractSystemFactory _coreFactory;

        public HH_SignalRComms(ContractSystemFactory factory)
        {
            _coreFactory = factory;
        }

        public override async Task OnConnectedAsync()
        {


        }

        public async Task<IEnumerable<EventContext>> GetAllSubscribableEvents()
        {
            return await _coreFactory.GetsubscribableEvents();
        }



    }
}
