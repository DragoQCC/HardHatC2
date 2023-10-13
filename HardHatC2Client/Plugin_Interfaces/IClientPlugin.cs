namespace HardHatC2Client.Plugin_Interfaces
{
    public interface IClientPlugin
    {
        Type GetComponentType();
    }

    public interface IClientPluginData
    {
        //name of the plugin, needs to be unique
        string Name { get; }       
        //details about the plugin 
        string Description { get; }
    }
}
