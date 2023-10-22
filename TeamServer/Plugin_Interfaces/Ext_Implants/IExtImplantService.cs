using System;
using HardHatCore.ApiModels.Plugin_Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Models;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants
{
    //should deal with the use of implants internally
    public interface IExtImplantService
    {
        public static readonly List<ExtImplant_Base> _IExtImplants = new();
        public static int ImplantNumber = 1;
        public static List<ExtImplant_Base> _extImplants = new();

        public static async Task ManageImplantStatusUpdate()
        {
            //for each implant if it has not checked in for 3 times the checkin interval then mark it as offline
            foreach (var implant in _extImplants)
            {
                if (implant.Metadata.Sleep > 0 && implant.LastSeen.AddSeconds(implant.Metadata.Sleep * 3) < DateTime.UtcNow && implant.Status is not "Offline")
                {
                    implant.Status = "Offline";
                    HardHatHub.AlertEventHistory(new HistoryEvent()
                    {
                        Event = $"Implant ({implant.ImplantType}) {implant.Metadata.Id} Offline",
                        Status = "Warning",
                    });
                    LoggingService.EventLogger.ForContext("Implant Metadata", implant.Metadata, true).ForContext("connection Type", implant.ConnectionType)
                        .Warning($"Implant ({implant.ImplantType}) {implant.Metadata.Id} Offline");

                    //update the implant in the database
                    if (DatabaseService.Connection != null)
                    {
                        DatabaseService.Connection.Update((ExtImplant_DAO)implant);
                    }
                    else
                    {
                        DatabaseService.Init();
                        DatabaseService.Connection.Update((ExtImplant_DAO)implant);
                    }
                }
                else
                {
                    if (implant.LastSeen.AddSeconds(60) < DateTime.UtcNow && implant.Status is not "Offline")
                    {
                        implant.Status = "Offline";
                        HardHatHub.AlertEventHistory(new HistoryEvent()
                        {
                            Event = $"Implant ({implant.ImplantType}) {implant.Metadata.Id} Offline",
                            Status = "Warning",
                        });
                        LoggingService.EventLogger.ForContext("Implant Metadata", implant.Metadata, true).ForContext("connection Type", implant.ConnectionType)
                            .Warning($"Implant ({implant.ImplantType}) {implant.Metadata.Id} Offline");
                    }
                }
            }
        }
        void AddExtImplant(ExtImplant_Base Implant);
        IEnumerable<ExtImplant_Base> GetExtImplants();
        ExtImplant_Base GetExtImplant(string id);
        void RemoveExtImplant(ExtImplant_Base Implant);
        bool CreateExtImplant(IExtImplantCreateRequest request, out string result_message);
        bool AddExtImplantToDatabase(ExtImplant_Base implant);
        Httpmanager GetImplantsManager(IExtImplantMetadata extImplantMetadata);
        ExtImplant_Base InitImplantObj(IExtImplantMetadata implantMeta, ref HttpContext httpcontentxt, string pluginName);
        ExtImplant_Base InitImplantObj(IExtImplantMetadata implantMeta, string pluginName);
        void LogImplantFirstCheckin(ExtImplant_Base implant);
        void UpdateImplantDBInfo(ExtImplant_Base implant);
        void GenerateUniqueEncryptionKeys(string implantId);
        byte[] EncryptImplantTaskData(byte[] bytesToEnc, string encryptionKey);
        byte[] DecryptImplantTaskData(byte[] bytesToDecrypt, string encryptionKey);
        byte[] HandleP2PDataDecryption(IExtImplant implant, byte[] encryptedData);
    }

    public interface IExtImplantServiceData : IPluginMetadata
    {

    }
}
