using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;
using HardHatCore.ContractorSystem.Contracts.ContractorCommTypes;

namespace HardHatCore.ApiModels.CoreSystems.InteractionSystem.Interactors.InteractorTypes
{
    internal class HHCore_Task_Consumer : IContractor
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ICommunicationDetails CommunicationDetails { get; set; }
    }
}
