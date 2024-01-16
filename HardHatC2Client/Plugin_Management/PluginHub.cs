using HardHatCore.HardHatC2Client.Plugin_BaseClasses;
using HardHatCore.HardHatC2Client.Plugin_Interfaces;
using HardHatCore.HardHatC2Client.Utilities;

namespace HardHatCore.HardHatC2Client.Plugin_Management
{
    public class PluginHub
    {
        public IEnumerable<IimplantCreation> ImplantCreation_Plugins { get; set; } = new List<IimplantCreation>();
        public IEnumerable<IImplantCommandValidation> ImplantTaskValidation_Plugins { get; set; } = new List<IImplantCommandValidation>();
        public IEnumerable<ICommandCodeView> CommandCodeView_Plugins { get; set; } = new List<ICommandCodeView>();
        public IEnumerable<IAssetNotificationService_Client> Asset_NotificationServicePlugins { get; set; } = new List<IAssetNotificationService_Client>();
    }
}
