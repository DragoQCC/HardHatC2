using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.HardHatC2Client.Pages;
using System.Diagnostics.CodeAnalysis;
using HardHatCore.HardHatC2Client.Plugin_Interfaces;

namespace HardHatCore.HardHatC2Client.Plugin_BaseClasses
{
    public class ExtImplant_Base : IExtImplant
    {
        public ExtImplantMetadata_Base Metadata { get; set; }
        public string ImplantType { get; set; }
        public int Number { get; set; }
        public string Note { get; set; }
        public string ConnectionType { get; set; }
        public string ExternalAddress { get; set; }
        public DateTime LastSeen { get; set; }
        public string LastSeenTimer { get; set; }
        public DateTime FirstSeen { get; set; }
        public string Status { get; set; }


        //can be overridden by a plugin
        public virtual void ModifyNoteFromContextChange(string command)
        {
            if (command.Equals("rev2self", StringComparison.CurrentCultureIgnoreCase))
            {
                Note = "";
                Metadata.Integrity = Metadata.Integrity;
            }
            else if (command.Equals("getsystem", StringComparison.CurrentCultureIgnoreCase))
            {
                Note = "Context: NT AUTHORITY\\SYSTEM";
                Metadata.Integrity = "SYSTEM";
            }
            else
            {
                Note = "Impersonating";
            }
        }

        public override int GetHashCode()
        {
            return Metadata.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ExtImplant_Base);
        }
        public bool Equals(ExtImplant_Base obj)
        {
            return obj != null && obj.Metadata.Id == this.Metadata.Id;
        }

    }
}
