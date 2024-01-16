using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.ContractorSystem.Contexts;
using HardHatCore.ContractorSystem.Contexts.Types;
using HardHatCore.ContractorSystem.Contractors.ContractorTypes;

namespace HardHatCore.ContractorSystem.Services
{
    internal static class ContractorSystem_Cache
    {
        public static List<IContextBase> Contexts = new List<IContextBase>();
        public static List<IContractor> Contractors = new List<IContractor>();
    }
}
