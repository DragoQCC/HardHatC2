using HardHatCore.HardHatC2Client.Plugin_Interfaces;

namespace HardHatCore.HardHatC2Client.Plugin_BaseClasses
{

    public class CommandCodeView_Base : ICommandCodeView
    {
        public string Name { get; set; } = "Default";

        //this is the related metadata for this plugin
        public ICommandCodeViewBaseData _metadata { get; set; } = new CommandCodeViewBaseData()
        {
            Name = "Default",
            Description = "This is the engineers data for the code view component",
            CodeLang = "cs",
            ImplantSrc = "../Engineer/Commands/"
        };

        public Type GetComponentType()
        {
            //just a placeholder this does not get called
            return typeof(ICommandCodeViewBaseData);
        }
    }

    public class CommandCodeViewBaseData : ICommandCodeViewBaseData
    {         
        public string Name { get; set; }
        public string Description { get; set; } 
        public string CodeLang { get; set; } 
        public string ImplantSrc { get; set; }
    }

    //Testing if a class can have a property when its exported if the type its trying to be exported as does not have that property
    public interface ICommandCodeView : IClientPlugin
    {
        ICommandCodeViewBaseData _metadata { get; set; }
        public Type GetComponentType();
    }

    public interface ICommandCodeViewBaseData : IClientPluginData
    {
        //blank because atm I just need the name and description from the parent but if i want to add more data to the plugin i can do it here
        public string CodeLang { get; }
        public string ImplantSrc { get; }
    }
}
