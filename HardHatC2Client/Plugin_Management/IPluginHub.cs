using HardHatCore.HardHatC2Client.Plugin_Interfaces;
using HardHatCore.HardHatC2Client.Plugin_BaseClasses;
using HardHatCore.HardHatC2Client.Utilities;

namespace HardHatCore.HardHatC2Client.Plugin_Management
{
    public interface IPluginHub
    {
        public IEnumerable<Lazy<IimplantCreation, ImplantCreationBaseData>> ImplantCreation_Plugins { get; }
        public IEnumerable<Lazy<ImplantCommandValidation_Base, ImplantCommandValidationBaseData>> ImplantTaskValidation_Plugins { get; }
        public IEnumerable<Lazy<ICommandCodeView, CommandCodeViewBaseData>> CommandCodeView_Plugins { get; }
        //public IEnumerable<Lazy<StandalonePage_Base, StandalonePageBaseData>> StandalonePage_Plugins { get; }
    }
}
