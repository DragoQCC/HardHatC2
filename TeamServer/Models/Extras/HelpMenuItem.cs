namespace TeamServer.Models.Extras;


public class HelpMenuItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Usage { get; set; }
    public bool NeedsAdmin { get; set; }
    public string MitreTechnique { get; set; }
    public OpsecStatus Opsec { get; set; }
    public string Details { get; set; }
    public string Keys { get; set; }

    //enum of opsec status 
    public enum OpsecStatus
    {
        NotSet,
        Low,
        Moderate,
        High,
        RequiresLeadAuthorization,
        Blocked
    }
}
