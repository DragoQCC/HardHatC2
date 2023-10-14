using SQLite;
using TeamServer.Models.Extras;

namespace TeamServer.Models.Dbstorage
{
    public class Webhooks_DAO
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Column("Name")]
        public string Name { get; set; }
        [Column("Url")]
        public string Url { get; set; }
        [Column("TargetSite")]
        public Webhook.TargetSites TargetSite { get; set; } //target site is used to determine the target template we will use
        [Column("Template")]
        public string Template { get; set; } //template content holds the scaffolded to send data to the specific webhook
        [Column("WebHookDataOption")]
        public Webhook.WebHookDataOptions WebHookDataOption { get; set; } //this is used to determine what data template we will use
        [Column("DataTemplate")]
        public string DataTemplate { get; set; }

        public static implicit operator Webhook(Webhooks_DAO dao)
        {
            return new Webhook
            {
                Id = dao.Id,
                Name = dao.Name,
                Url = dao.Url,
                TargetSite = dao.TargetSite,
                Template = dao.Template,
                WebHookDataOption = dao.WebHookDataOption,
                DataTemplate = dao.DataTemplate
            };
        }

        public static implicit operator Webhooks_DAO(Webhook webhook)
        {
            return new Webhooks_DAO
            {
                Id = webhook.Id,
                Name = webhook.Name,
                Url = webhook.Url,
                TargetSite = webhook.TargetSite,
                Template = webhook.Template,
                WebHookDataOption = webhook.WebHookDataOption,
                DataTemplate = webhook.DataTemplate
            };
        }
    }
}
