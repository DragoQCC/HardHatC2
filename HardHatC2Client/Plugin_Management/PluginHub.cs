using HardHatCore.HardHatC2Client.Plugin_Interfaces;
using System.ComponentModel.Composition;
using HardHatCore.HardHatC2Client.Plugin_BaseClasses;
using HardHatCore.HardHatC2Client.Utilities;

namespace HardHatCore.HardHatC2Client.Plugin_Management
{
    [Export(typeof(IPluginHub))]
    public class PluginHub : IPluginHub
    {
        [ImportMany]
        public IEnumerable<Lazy<IimplantCreation, ImplantCreationBaseData>> ImplantCreation_Plugins { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<ImplantCommandValidation_Base, ImplantCommandValidationBaseData>> ImplantTaskValidation_Plugins { get; set; }

        [ImportMany]
        public IEnumerable<Lazy<ICommandCodeView, CommandCodeViewBaseData>> CommandCodeView_Plugins { get; set; }
    }
}
