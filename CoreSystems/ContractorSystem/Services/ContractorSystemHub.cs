using HardHatCore.ContractorSystem.Contexts.Types;
using Microsoft.AspNetCore.SignalR;

namespace HardHatCore.ContractorSystem.Services
{
    public partial class ContractorSystemHub : Hub
    {
        public override async Task OnConnectedAsync()
        {


        }

        #region Client Invokable Methods 
        public async Task<IEnumerable<EventContext>> GetAllSubscribableEvents()
        {
            return await ContractSystemFactory.GetSubableEvents();
        }



        #endregion

    }
}
