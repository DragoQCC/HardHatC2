using HardHatC2Client.Plugin_Interfaces;
using System.ComponentModel.Composition;

namespace HardHatC2Client.Plugin_BaseClasses
{
    // [Export(typeof(CommandCodeView_Base))]
    [Export(typeof(ICommandCodeView))] // example where this class has a property that its exported type does not 
    [ExportMetadata("Name", "Default")]
    [ExportMetadata("Description", "This is the engineers data for the code view component")]
    //the file ending for the source files in this case cs for c#
    [ExportMetadata("CodeLang", "cs")]
    //relative path from HardHatC2\HardHatC2Client to the folder holding the command source files
    [ExportMetadata("ImplantSrc", "../Engineer/Commands/")]
    public class CommandCodeView_Base : ICommandCodeView
    {
        public string Name { get; set; } = "Default";
        public Type GetComponentType()
        {
            //just a placeholder this does not get called
            return typeof(CommandCodeViewBaseData);
        }
    }

    //Testing if a class can have a property when its exported if the type its trying to be exported as does not have that property
    public interface ICommandCodeView : IClientPlugin
    {
        public Type GetComponentType();
    }

    public interface CommandCodeViewBaseData : IClientPluginData
    {
        //blank because atm I just need the name and description from the parent but if i want to add more data to the plugin i can do it here
        public string CodeLang { get; }
        public string ImplantSrc { get; }
    }
}
