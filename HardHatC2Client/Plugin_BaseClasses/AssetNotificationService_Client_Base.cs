using HardHatCore.ApiModels.Shared;
using HardHatCore.HardHatC2Client.Plugin_Interfaces;

namespace HardHatCore.HardHatC2Client.Plugin_BaseClasses
{
    public class AssetNotificationService_Client_Base : IAssetNotificationService_Client
    {
        public string Name { get; set; } = "Default";

        public IAssetNotificationServiceData _metadata { get; set; } = new AssetNotificationServiceData()
        {
            Name = "Default",
            Description = "Base class for AssetNotificationService_Client plugins"
        };

        public async Task ProcessAssetNotification(AssetNotification assetNotif)
        {
            throw new NotImplementedException();
        }

        public async Task CreateAndQueueAssetNotif(string AssetId, string NotificationName, Dictionary<string, byte[]> NotificationData)
        {
            throw new NotImplementedException();
        }
    }

    public class AssetNotificationServiceData : IAssetNotificationServiceData
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
