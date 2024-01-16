using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Shared;
using HardHatCore.ApiModels.Shared.TaskResultTypes;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Utilities;

namespace HardHatCore.TeamServer.Plugin_BaseClasses
{
    public class AssetNotificationService_Base : IAssetNotificationService
    {
        public string Name { get; } = "Default";
        public string Description { get; } = "Default asset notification processing class";
        
        public async Task CreateAndQueueAssetNotif(string AssetId, string NotificationName, Dictionary<string, byte[]> NotificationData, bool forwardtoclient = false)
        {
            //create the notification
            AssetNotification notif = new AssetNotification()
            {
                AssetId = AssetId,
                NotificationName = NotificationName,
                NotificationData = NotificationData,
                ForwardToClient = forwardtoclient
            };
            //find the asset/Implant based on the AssetId
            ExtImplant_Base asset = await DatabaseService.GetExtImplant(AssetId);
            //queue the notification
            asset.assetNotifications.Enqueue(notif);
        }

        public async Task ProcessAssetNotification(AssetNotification assetNotif)
        {
            //as we make more notifications we will need to add more cases here, should call based off the name of the notification
            if (assetNotif.NotificationName.Equals("SocksConnect", StringComparison.CurrentCultureIgnoreCase))
            {
                await HandleNotif_SocksConnect(assetNotif);
            }
            else if (assetNotif.NotificationName.Equals("socksReceive", StringComparison.CurrentCultureIgnoreCase))
            {
                await HandleNotif_SocksReceive(assetNotif);
            }
            if (assetNotif.NotificationName.Equals("VNCHeartbeat",StringComparison.CurrentCultureIgnoreCase))
            {
                await HandleNotif_VNCHeartbeat(assetNotif);
            }
        }

        public static async Task HandleNotif_SocksConnect(AssetNotification notif)
        {
            try
            {
                string sockclientId = notif.NotificationData.DeserializeAssetNotifValue<string>("/client");
                string proxyID = IExtimplantHandleComms.SocksClientToProxyCache[sockclientId];
                IExtimplantHandleComms.Proxy[proxyID].SocksDestinationConnected[sockclientId] = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static async Task HandleNotif_SocksReceive(AssetNotification notif)
        {
            try
            {
                var socks_client =  notif.NotificationData.DeserializeAssetNotifValue<string>("/client");
                var socks_content = notif.NotificationData["/data"];
                string proxyID = IExtimplantHandleComms.SocksClientToProxyCache[socks_client];
                IExtimplantHandleComms.Proxy[proxyID].SocksClientsData[socks_client].Enqueue(socks_content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private async Task HandleNotif_VNCHeartbeat(AssetNotification notif)
        {
            
        }
    }
}
