﻿using HardHatCore.ApiModels.Plugin_Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Plugin_Management;
using HardHatCore.TeamServer.Services;
using HardHatCore.TeamServer.Utilities;

namespace HardHatCore.TeamServer.Plugin_BaseClasses
{
    [Export(typeof(IExtImplantService))]
    [ExportMetadata("Name","Default")]
    public class ExtImplantService_Base : IExtImplantService
    {
        
        public virtual void AddExtImplant(ExtImplant_Base Implant)
        {
            IExtImplantService._extImplants.Add(Implant);
        }
        public virtual void RemoveExtImplant(ExtImplant_Base Implant)
        {
            IExtImplantService._extImplants.Remove(Implant);
        }

        public virtual bool DeleteAssetById(string id)
        {
            var asset = IExtImplantService._extImplants.FirstOrDefault(x => x.Metadata.Id == id);
            if (asset != null)
            {
                IExtImplantService._extImplants.Remove(asset);
                RemoveAssetFromDatabase(id);
                return true;
            }
            return false;
        }

        public virtual bool CreateExtImplant(IExtImplantCreateRequest request, out string result_message)
        {
           //3rd party devs will have to override this and provide their own code to create the implant
           return EngineerService.CreateEngineers(request, out result_message);
        }     
        
        public virtual Httpmanager GetImplantsManager(IExtImplantMetadata extImplantMetadata)
        {
            var manager = managerService._managers.FirstOrDefault(x => x.Name == extImplantMetadata.ManagerName);
            if (manager == null) {
                return null;
            }
            else
            {
                return (Httpmanager)manager;
            }
        }
        public virtual IEnumerable<ExtImplant_Base> GetExtImplants()
        {
            return IExtImplantService._extImplants;
        }

        public virtual ExtImplant_Base GetExtImplant(string id)
        {
            return IExtImplantService._extImplants.FirstOrDefault(x => x.Metadata.Id == id);
        }

        public virtual ExtImplant_Base InitImplantObj(IExtImplantMetadata implantMeta, ref HttpContext httpcontentxt,string pluginName)
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

        public virtual bool RemoveAssetFromDatabase(string id)
        {
            try
            {
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }
                DatabaseService.AsyncConnection.DeleteAsync<ExtImplant_DAO>(id);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error removing implant from database");
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
            if(!DatabaseService.ImplantLastDatabaseUpdateTime.ContainsKey(implant.Metadata.Id))
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
            //taskEncryptionKey = Convert.ToBase64String(GeneratePasswordBytes(taskEncryptionKey));
            if (!Encryption.UniqueTaskEncryptionKey.ContainsKey(implantId))
            {
                Encryption.UniqueTaskEncryptionKey.Add(implantId, taskEncryptionKey);
            }
            else
            {
                Encryption.UniqueTaskEncryptionKey[implantId] = taskEncryptionKey;
            }
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync(new EncryptionKeys_DAO() { ItemID = implantId, Key = taskEncryptionKey });
        }

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
                //Console.WriteLine(ex.Message);
                return null;
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
