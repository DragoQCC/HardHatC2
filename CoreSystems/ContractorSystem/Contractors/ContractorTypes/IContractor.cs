
using System.Runtime.CompilerServices;
using HardHatCore.ContractorSystem.Contexts.Types;
using HardHatCore.ContractorSystem.Contractors.ContractorCommTypes;

namespace HardHatCore.ContractorSystem.Contractors.ContractorTypes
{
    public interface IContractor
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Id { get; set; }
        public ICommunicationDetails CommunicationDetails { get; set; }
        public IEnumerable<IContextBase>? RelatedContexts { get; set; }

    }
}
