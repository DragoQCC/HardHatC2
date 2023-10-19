using System.Collections.Generic;

namespace HardHatCore.TeamServer.Models.Extras
{
    public class Webhook
    {
        public static List<Webhook> ExistingWebhooks = new();

        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public TargetSites TargetSite { get; set; } //target site is used to determine the target template we will use 
        public string Template { get; set; } //template content holds the scaffolded to send data to the specific webhook
        public WebHookDataOptions WebHookDataOption { get; set; } //this is used to determine what data template we will use
        public string DataTemplate { get; set; }

        public enum TargetSites
        {
            Discord,
            Slack,
            MatterMost,
            Custom
        }

        public enum WebHookDataOptions
        {
            Custom,
            NewCheckIn,
            Alert,
            Initilization,
            Feedback,
        }
    }
}
