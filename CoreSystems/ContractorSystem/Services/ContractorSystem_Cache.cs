using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ContractorSystem.Contexts;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;
using HardHatCore.ContractorSystem.Contracts;

namespace HardHatCore.ContractorSystem.Services
{
    internal static class ContractorSystem_Cache
    {
        public static List<Contract_Base> Contracts = new List<Contract_Base>();
        public static List<IContextBase> Contexts = new List<IContextBase>();
        public static List<IContractor> Contractors = new List<IContractor>();
    }
}
