using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HardHatCore.ApiModels.Aspects;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Utilities;
using Microsoft.AspNetCore.Http;

namespace HardHatCore.TeamServer.Plugin_BaseClasses
{
    public class ExtImplantService_Base : IExtImplantService
    {
        public string Name { get; } = "Default";
        public string Description { get; } = "Default implant service";
        

        public virtual void AddExtImplant(ExtImplant_Base Implant)
        {
            IExtImplantService._extImplants.Add(Implant);
        }
        public virtual void RemoveExtImplant(ExtImplant_Base Implant)
        {
            IExtImplantService._extImplants.Remove(Implant);
        }

        public virtual bool CreateExtImplant(IExtImplantCreateRequest request, out string result_message)
        {
            //3rd party devs will have to override this and provide their own code to create the implant
            return EngineerService.CreateEngineers(request, out result_message);
        }

        public virtual HttpManager GetImplantsManager(IExtImplantMetadata extImplantMetadata)
        {
            
            var manager = ImanagerService.Getmanager(extImplantMetadata.ManagerName);
            if (manager == null) {
                return null;
            }
            else
            {
                return (HttpManager)manager;
            }
        }


        public virtual ExtImplant_Base InitImplantObj(IExtImplantMetadata implantMeta, ref HttpContext httpcontentxt, string pluginName)
        {
            var model = PluginService.GetImpPlugin(pluginName);
            IExtImplantService.ImplantNumber++;
            var temp = model.GetNewIExtImplant(implantMeta);
            temp.ConnectionType = httpcontentxt.Request.Scheme;
            temp.ExternalAddress = httpcontentxt.Connection.RemoteIpAddress.ToString();
            temp.ImplantType = pluginName;
            return temp;
        }

        public virtual ExtImplant_Base InitImplantObj(IExtImplantMetadata implantMeta, string pluginName)
        {
            var model = PluginService.GetImpPlugin(pluginName);
            var temp = model.GetNewIExtImplant(implantMeta);
            temp.ImplantType = pluginName;
            return temp;
        }

        public virtual bool AddExtImplantToDatabase(ExtImplant_Base implant)
        {
            try
            {
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }
                DatabaseService.AsyncConnection.InsertAsync((ExtImplant_DAO)implant);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding implant to database");
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public virtual void LogImplantFirstCheckin(ExtImplant_Base implant)
        {
            HardHatHub.AlertEventHistory(new HistoryEvent()
            {
                Event = $"Implant ({implant.ImplantType}) {implant.Metadata.Id} Checkin",
                Status = "Success",
            });

            LoggingService.EventLogger.ForContext("Implant Metadata", implant.Metadata, true).ForContext("connection Type", implant.ConnectionType)
                .Information($"Implant ({implant.ImplantType}) {implant.Metadata.ProcessId}@{implant.Metadata.Address} checked in for the first time");
        }

        public virtual void UpdateImplantDBInfo(ExtImplant_Base implant)
        {
            if (!DatabaseService.ImplantLastDatabaseUpdateTime.ContainsKey(implant.Metadata.Id))
            {
                DatabaseService.ImplantLastDatabaseUpdateTime.Add(implant.Metadata.Id, DateTime.UtcNow);
            }
            //should hopefully prevent the database from being spammed with updates if the implant is checking in every 30 seconds or more
            if (implant.Metadata.Sleep >= 30)
            {
                DatabaseService.ImplantLastDatabaseUpdateTime[implant.Metadata.Id] = DateTime.UtcNow;
                DatabaseService.AsyncConnection.UpdateAsync((ExtImplant_DAO)implant);
            }
            //now it should only interact with the database at a minimum of every 30 seconds
            else if (implant.Metadata.Sleep < 30 && DatabaseService.ImplantLastDatabaseUpdateTime[implant.Metadata.Id].AddSeconds(30) < DateTime.UtcNow)
            {
                DatabaseService.ImplantLastDatabaseUpdateTime[implant.Metadata.Id] = DateTime.UtcNow;
                DatabaseService.AsyncConnection.UpdateAsync((ExtImplant_DAO)implant);
            }
            else
            {
                return;
            }
        }

        public virtual void GenerateUniqueEncryptionKeys(string implantId)
        {
            // generate a random key for the task encryption key
            string taskEncryptionKey = Encryption.GenerateRandomString(32);
            Encryption.UniqueTaskEncryptionKey[implantId] = taskEncryptionKey;
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync(new EncryptionKeys_DAO() { ItemID = implantId, Key = taskEncryptionKey });
        }

        [InvocableInternalOnly]
        public virtual byte[] EncryptImplantTaskData(byte[] bytesToEnc, string encryptionKey)
        {
            try
            {
                byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
                byte[] encryptedBytes = null;
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using MemoryStream ms = new MemoryStream();
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 500, HashAlgorithmName.SHA256);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToEnc, 0, bytesToEnc.Length);
                        cs.Close();
                    }

                    aes.Clear();
                }
                encryptedBytes = ms.ToArray();
                return encryptedBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        [InvocableInternalOnly]
        public virtual byte[] DecryptImplantTaskData(byte[] bytesToDecrypt, string encryptionKey)
        {
            try
            {
                byte[] passwordBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
                byte[] decryptedBytes = null;
                byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                using MemoryStream ms = new MemoryStream();
                using (Aes aes = Aes.Create())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    //var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 500, HashAlgorithmName.SHA256);
                    aes.Key = key.GetBytes(aes.KeySize / 8);
                    aes.IV = key.GetBytes(aes.BlockSize / 8);
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToDecrypt, 0, bytesToDecrypt.Length);
                        cs.Close();
                    }
                    aes.Clear();
                }
                decryptedBytes = ms.ToArray();
                return decryptedBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        //Intent is to allow for custom messages to be sent to the implant, since HardHatCore cannot know what the implant will be expecting it is up to the developer to gather those items and return a byte[] to be packaged 
        // the returned byte[] should be a serialized list 
        //desired implant code example 
        // var _ExtImplantService_Base = new();
        //public byte[] GetOutboundCustomMessage(ExtImplant_Base asset) {return _ExtImplantService_Base(asset);}
        //then during this flow if it hits a function that is internal only it would try to invoke the function from the plugins implementation of the interface
        public virtual byte[] GetOutboundCustomMessage(ExtImplant_Base asset)
        {
            var customMessageCollection = GetCustomMessageDictImp();
            return CreateSerializedC2MessageHelper(customMessageCollection, asset);
        }

        [InvocableInternalOnly]
        public virtual Dictionary<int, List<byte[]>> GetCustomMessageDictImp()
        {
            return new Dictionary<int, List<byte[]>>();
        }

        //takes in a Dictionary of custom message numbers and associated serialized objects and returns a serialized byte[] of C2Messages to be sent to the implant
        public static byte[] CreateSerializedC2MessageHelper(Dictionary<int, List<byte[]>> customMessageCollection, ExtImplant_Base asset)
        {
            var extImplantService_Base = PluginService.GetImpServicePlugin(asset.ImplantType);
            List<C2Message> messageList = new List<C2Message>();
            foreach (var customMessage in customMessageCollection)
            {
                if (customMessage.Value.Any())
                {
                    byte[] encryptedDataArray = Array.Empty<byte>();
                    byte[] messageDataArray = Array.Empty<byte>();
                    customMessage.Value.ForEach(x => messageDataArray.Concat(x));
                    if (messageDataArray != null && messageDataArray.Length > 0)
                    {
                        if (Encryption.UniqueTaskEncryptionKey.ContainsKey(asset.Metadata.Id))
                        {
                            encryptedDataArray = extImplantService_Base.EncryptImplantTaskData(messageDataArray, Encryption.UniqueTaskEncryptionKey[asset.Metadata.Id]);
                        }
                        else
                        {
                            encryptedDataArray = extImplantService_Base.EncryptImplantTaskData(messageDataArray, Encryption.UniversalTaskEncryptionKey);
                        }
                    }
                    messageList.Add(new C2Message() { MessageType = customMessage.Key, Data = encryptedDataArray, PathMessage = IExtimplantHandleComms.P2P_PathStorage[asset.Metadata.Id] });
                }
            }
            if (messageList.Count > 0)
            {
                return messageList.Serialize();
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        public virtual byte[] HandleP2PDataDecryption(IExtImplant implant, byte[] encryptedData)
        {
            byte[] decryptedBytes = null;
            if (IExtimplantHandleComms.P2P_PathStorage.ContainsKey(implant.Metadata.Id))
            {
                foreach (string key in IExtimplantHandleComms.P2P_PathStorage[implant.Metadata.Id])
                {
                    if (Encryption.UniqueTaskEncryptionKey.ContainsKey(key))
                    {
                        decryptedBytes = DecryptImplantTaskData(encryptedData, Encryption.UniqueTaskEncryptionKey[key]);
                        if (decryptedBytes != null)
                        {
                            break;
                        }
                    }
                    decryptedBytes = DecryptImplantTaskData(encryptedData, Encryption.UniversalTaskEncryptionKey);
                }
            }
            return decryptedBytes;
        }
    }
}
