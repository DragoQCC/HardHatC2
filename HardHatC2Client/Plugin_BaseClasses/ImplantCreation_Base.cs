using HardHatCore.HardHatC2Client.Plugin_Interfaces;
using HardHatCore.HardHatC2Client.Components.ImplantCreation;
using Microsoft.AspNetCore.Components;


namespace HardHatCore.HardHatC2Client.Plugin_BaseClasses
{
    //this base class is also the implementation for the Engineer as an example of how to create a plugin
    public class ImplantCreation_Base : IimplantCreation
    {
        //this is the metadata for the plugin related to different supported features by the Asset
        public I_ImplantCreationBaseData _metadata { get; set; } = new ImplantCreation_BaseData
        {
            Name = "Default",
            Description = "This is the built in Implant Creation Page, override this if you need custom options when compiling your implant",
            SupportedCommTypes = new string[] { "HTTP", "HTTPS", "TCP", "SMB" },
            SupportedOperatingSystems = new string[] { "Windows" },
            SupportedOutputTypes = new string[] { "exe", "serviceExe", "dll", "powershellCmd", "bin" },
            SupportsConnectionAttempts = true,
            SupportsKillDates = true,
            SupportsPostEx = true
        };

        public string Name { get; set; } = "Default";

        //the matching razor component for this plugin
        public virtual Type GetComponentType()
        {
            //this should return the type of the razor page component we want to render in the UI
            return typeof(ImplantCreation_PluginContent);
        }

        //optional but if you want custom Module Options UI for your plugin you can override this method and return a RenderFragment
        public virtual RenderFragment GetModuleOptionsUI()
        {
            return builder =>
            {
                builder.OpenComponent(0, typeof(ModuleOptionsComponent));
                builder.CloseComponent();
            };
        }
    }




    public class ImplantCreation_BaseData : I_ImplantCreationBaseData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] SupportedCommTypes { get; set; }
        public string[] SupportedOperatingSystems { get; set; }
        public string[] SupportedOutputTypes { get; set; }
        public bool SupportsConnectionAttempts { get; set; }
        public bool SupportsKillDates { get; set; }
        public bool SupportsPostEx { get; set; }
    }

    public interface IimplantCreation : IClientPlugin
    {
        I_ImplantCreationBaseData _metadata { get; set; }
        RenderFragment GetModuleOptionsUI();
        Type GetComponentType();
    }


    public interface I_ImplantCreationBaseData : IClientPluginData
    {
        //used to set the allowed comm types to select in the UI current options are : HTTP, HTTPS, TCP, SMB
        string[] SupportedCommTypes { get; set; }
        //sets the operating systems the implant can run on, current options are : Windows, Linux, MacOS
        string[] SupportedOperatingSystems { get; set; }
        //sets the output types the implant can be compiled as options are : exe, serviceExe, dll, powershellCmd, bin
        string[] SupportedOutputTypes { get; set; }
        //enables or disables the ability to set a number of connection attempts before the implant will stop trying to connect
        bool SupportsConnectionAttempts { get; set; }
        //enables or disables the ability to set a date and time in UTC at which the implant will stop running
        bool SupportsKillDates { get; set; }
        //enables or disables the ability to build the implant for a postex command such as Jump or Inject 
        bool SupportsPostEx { get; set; }
    }

   
}
