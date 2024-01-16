using HardHatCore.ContractorSystem.Contexts.Types;
using HardHatCore.ContractorSystem.Contractors.ContractorCommTypes;

namespace HardHatCore.ContractorSystem.Contractors.ContractorTypes
{
    public class Contractor_Base : IContractor
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Id { get; set; }
        public ICommunicationDetails CommunicationDetails { get; set; }
        public List<IContextBase> RelatedContexts { get; set; } = new List<IContextBase>();
    }
}
