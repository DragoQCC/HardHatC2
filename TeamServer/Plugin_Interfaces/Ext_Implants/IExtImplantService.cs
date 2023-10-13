using ApiModels.Plugin_Interfaces;
using System.Collections.Generic;
using TeamServer.Plugin_BaseClasses;

namespace TeamServer.Plugin_Interfaces.Ext_Implants
{
    //should deal with the use of implants internally
    public interface IExtImplantService
    {
        public static readonly List<ExtImplant_Base> _IExtImplants = new();
        void AddExtImplant(ExtImplant_Base Implant);
        IEnumerable<ExtImplant_Base> GetExtImplants();
        ExtImplant_Base GetExtImplant(string id);
        void RemoveExtImplant(ExtImplant_Base Implant);
        public bool CreateExtImplant(IExtImplantCreateRequest request, out string result_message);
        public bool AddExtImplantToDatabase(ExtImplant_Base implant);
    }

    public interface IExtImplantServiceData : IPluginMetadata
    {

    }
}
