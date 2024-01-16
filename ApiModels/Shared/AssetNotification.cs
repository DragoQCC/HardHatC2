using System;
using System.Collections.Generic;

namespace HardHatCore.ApiModels.Shared
{
    [Serializable]
    public class AssetNotification
    {
        //Id of the Notification
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        //Id of the Implant / Asset the Notification is for / from
        public string AssetId { get; set; }
        
        //The name of the Notification to trigger on
        public string NotificationName { get; set; }

        //The type of the Notification to trigger on
        //optional but can be used to filter & group notifications if desired
        public int? NotificationType { get; set; }
        
        //Collection of string / byte[] pairs to be used as the data sent with the notification such as important Ids, Traffic, etc.
        public Dictionary<string, byte[]> NotificationData { get; set; } = new Dictionary<string, byte[]>();

        //This is set by deserializing data from an Asset and is used to determine if the notification should be forwarded to the client,
        //ex. of a task to forward => A Task Cancel notification
        //ex. of a task not to forward => socks traffic  
        public bool ForwardToClient { get; set; } = false;
    }
}
