using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ContractorSystem.Contexts;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;
using HardHatCore.ContractorSystem.Contracts.ContractorCommTypes;

namespace HardHatCore.ContractorSystem.Contracts
{
    public class Contract_Base
    {
        //unique Id for the contract
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        // description of the contract to help identify / understand it
        public string Description { get; set; }
        
        //the member that created the contract
        public IContractor ContractOriginator {  get; set; }
        
        //the member that signed the contract
        public IContractor ContractSigner { get; set; }
      
        //this holds the List of ActivationContexts this contract is for
        public List<IContextBase> ContractActions { get; set; } = new();
    }
}
