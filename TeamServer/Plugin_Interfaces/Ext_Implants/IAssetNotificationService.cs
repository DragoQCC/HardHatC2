using System.Collections.Generic;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Shared;

namespace HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants
{
    public interface IAssetNotificationService : IAssetNotificationServiceData
    {
        Task ProcessAssetNotification(AssetNotification assetNotif);
        Task CreateAndQueueAssetNotif(string AssetId,string NotificationName, Dictionary<string, byte[]> NotificationData, bool forwardtoclient = false);
    }

    public interface IAssetNotificationServiceData : IPluginMetadata
    {

    }
}
