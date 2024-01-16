using HardHatCore.ApiModels.Shared;

namespace HardHatCore.HardHatC2Client.Plugin_Interfaces
{
    public interface IAssetNotificationService_Client : IClientPlugin
    {
        IAssetNotificationServiceData _metadata { get; set; }
        Task ProcessAssetNotification(AssetNotification assetNotif);
        Task CreateAndQueueAssetNotif(string AssetId, string NotificationName, Dictionary<string, byte[]> NotificationData);
        
    }


    public interface IAssetNotificationServiceData : IClientPluginData
    {

    }
}
