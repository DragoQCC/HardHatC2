namespace HardHatCore.HardHatC2Client.Plugin_Interfaces
{
    public interface IClientPlugin
    {
        string Name { get; set; }
    }

    public interface IClientPluginData
    {
        //name of the plugin, needs to be unique
        string Name { get; set; }       
        //details about the plugin 
        string Description { get; set; }
    }
}
