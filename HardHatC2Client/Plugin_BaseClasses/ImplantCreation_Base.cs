using HardHatC2Client.Components;
using HardHatC2Client.Plugin_Interfaces;
using System.ComponentModel.Composition;

namespace HardHatC2Client.Plugin_BaseClasses
{
    [Export(typeof(ImplantCreation_Base))]
    [ExportMetadata("Name", "Default")]
    [ExportMetadata("Description", "This is the built in Implant Creation Page, override this if you neeed custom options when compiling your implant")]
    public class ImplantCreation_Base : IClientPlugin
    {
        public virtual Type GetComponentType()
        {
            //this should return the type of the razor page component we want to render in the UI
            return typeof(ImplantCreation_PluginContent);
        }
    }

    public interface ImplantCreationBaseData : IClientPluginData
    {
        //blank because atm I just need the name and description from the parent but if i want to add more data to the plugin i can do it here
    }
}
