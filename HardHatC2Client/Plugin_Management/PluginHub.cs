using HardHatC2Client.Plugin_BaseClasses;
using HardHatC2Client.Plugin_Interfaces;
using HardHatC2Client.Utilities;
using System.ComponentModel.Composition;

namespace HardHatC2Client.Plugin_Management
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
