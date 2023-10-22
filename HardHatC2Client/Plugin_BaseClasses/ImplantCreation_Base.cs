using HardHatCore.HardHatC2Client.Components;
using System.ComponentModel.Composition;
using HardHatCore.HardHatC2Client.Components.ImplantCreation;
using HardHatCore.HardHatC2Client.Plugin_Interfaces;
using Microsoft.AspNetCore.Components;

namespace HardHatCore.HardHatC2Client.Plugin_BaseClasses
{
    //this base class is also the implementation for the Engineer as an example of how to create a plugin
    [Export(typeof(IimplantCreation))]
    [ImplantCreationBaseData
        (
            Name = "Default",
            Description = "This is the built in Implant Creation Page, override this if you need custom options when compiling your implant",
            SupportedCommTypes = new string[] { "HTTP", "HTTPS", "TCP", "SMB" },
            SupportedOperatingSystems = new string[] { "Windows" },
            SupportedOutputTypes = new string[] { "exe", "serviceExe", "dll", "powershellCmd", "bin" },
            SupportsConnectionAttempts = true,
            SupportsKillDates = true,
            SupportsPostEx = true
        )
    ]
    public class ImplantCreation_Base : IimplantCreation
    {

        public virtual Type GetComponentType()
        {
            //this should return the type of the razor page component we want to render in the UI
            return typeof(ImplantCreation_PluginContent);
        }

        public virtual RenderFragment GetModuleOptionsUI()
        {
            return builder =>
            {
                builder.OpenComponent(0, typeof(ModuleOptionsComponent));
                builder.CloseComponent();
            };
        }
    }

    
    
    [MetadataAttribute]
    public class ImplantCreationBaseDataAttribute : Attribute
    {
        //name of the plugin, needs to be unique use the same name on any exports in the same project 
        public string Name { get; set; }
        //details about the plugin not used for any logic so can be whatever you want
        public string Description { get; set; }
        //used to set the allowed comm types to select in the UI current options are : HTTP, HTTPS, TCP, SMB
        public string[] SupportedCommTypes { get; set; }
        //sets the operating systems the implant can run on, current options are : Windows, Linux, MacOS
        public string[] SupportedOperatingSystems { get; set; }
        //sets the output types the implant can be compiled as options are : exe, serviceExe, dll, powershellCmd, bin
        public string[] SupportedOutputTypes { get; set; }
        //enables or disables the ability to set a number of connection attempts before the implant will stop trying to connect
        public bool SupportsConnectionAttempts { get; set; }
        //enables or disables the ability to set a date and time in UTC at which the implant will stop running
        public bool SupportsKillDates { get; set; }
        //enables or disables the ability to build the implant for a postex command such as Jump or Inject 
        public bool SupportsPostEx { get; set; }
    }

    public interface IimplantCreation : IClientPlugin
    {
        RenderFragment GetModuleOptionsUI();
        Type GetComponentType();
    }


    public interface ImplantCreationBaseData : IClientPluginData
    {
        //used to set the allowed comm types to select in the UI current options are : HTTP, HTTPS, TCP, SMB
        string[] SupportedCommTypes { get; }
        //sets the operating systems the implant can run on, current options are : Windows, Linux, MacOS
        string[] SupportedOperatingSystems { get; }
        //sets the output types the implant can be compiled as options are : exe, serviceExe, dll, powershellCmd, bin
        string[] SupportedOutputTypes { get; }
        //enables or disables the ability to set a number of connection attempts before the implant will stop trying to connect
        bool SupportsConnectionAttempts { get; }
        //enables or disables the ability to set a date and time in UTC at which the implant will stop running
        bool SupportsKillDates { get;  }
        //enables or disables the ability to build the implant for a postex command such as Jump or Inject 
        bool SupportsPostEx { get; }
    }

   
}
