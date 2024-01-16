using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.TeamServer.Models;
using HardHatCore.TeamServer.Models.Database;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Models.Extras;
using HardHatCore.TeamServer.Models.Managers;
using HardHatCore.TeamServer.Plugin_BaseClasses;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Utilities;
using SQLite;

namespace HardHatCore.TeamServer.Services
{
    public class DatabaseService
    {
        public static string ConnectionString = null;
        public static SQLiteAsyncConnection AsyncConnection = null;
        public static SQLiteConnection Connection = null;
        private static List<Type> dbItemTypes = new List<Type> { typeof(Cred_DAO), typeof(DownloadFile_DAO), typeof(EncryptionKeys_DAO), typeof(ExtImplant_DAO), typeof(ExtImplantTaskResult_DAO), typeof(HistoryEvent_DAO),
                                                        typeof(HttpManager_DAO), typeof(PivotProxy_DAO),typeof(ReconCenterEntity_DAO), typeof(SMBManager_DAO), typeof(TCPManager_DAO), typeof(UploadedFile_DAO),typeof(ExtImplantTask_DAO),
                                                typeof(IOCFIle_DAO),typeof(CompiledImplant_DAO),typeof(Alias_DAO),typeof(Webhooks_DAO) };

        public static Dictionary<string,DateTime> ImplantLastDatabaseUpdateTime { get; set; } = new Dictionary<string, DateTime>();
        
        //create a function to init the database file, location string, and setup tables 
        public static void Init()
        {
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            // find the ExtImplant cs file and load it to a string so we can update it and then run the compiler function on it
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //split path at bin keyword
            string[] pathSplit = path.Split("bin"); //[0] is the parent folder [1] is the bin folder
            //update each string in the array to replace \\ with the correct path seperator
            pathSplit[0] = pathSplit[0].Replace("\\", allPlatformPathSeperator.ToString());

            //make a folder named database at pathSplit[0] if one does not exist 
            if (!Directory.Exists(pathSplit[0] + allPlatformPathSeperator + "database"))
            {
                Directory.CreateDirectory(pathSplit[0] + allPlatformPathSeperator + "database");
            }
            string databaseDirectory = pathSplit[0] + "database";
            //make a db file named HardHarC2.db inside the databaseDirectory 
            if (!File.Exists(databaseDirectory + allPlatformPathSeperator + "HardHatC2.db"))
            {
                File.Create(databaseDirectory + allPlatformPathSeperator + "HardHatC2.db").Close();
            }
            //create a connection string to the database file
            ConnectionString = databaseDirectory + allPlatformPathSeperator + "HardHatC2.db";
            Thread.Sleep(1000);
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        //create a connection to the database file
        public static void ConnectDb()
        {
            try
            {
                if (ConnectionString == null)
                {
                    Init();
                }
                if (AsyncConnection == null)
                {

                    Connection = new SQLiteConnection(ConnectionString);
                    AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
                }
                Console.WriteLine("Connected to sqlite server");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error connecting to sqlite server: " + ex.Message);
            }
        }

        public static async Task CreateTables()
        {
            if (AsyncConnection == null)
            {
                Connection = new SQLiteConnection(ConnectionString);
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            foreach (Type type in dbItemTypes)
            {
               await AsyncConnection.CreateTableAsync(type);
            }
            await AsyncConnection.CreateTablesAsync<UserInfo, RoleInfo, UserRoleInfo, UserSalt>();
        }

        public static async Task CreateTable(Type tableType)
        {
            if (AsyncConnection == null)
            {
                Connection = new SQLiteConnection(ConnectionString);
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            await AsyncConnection.CreateTableAsync(tableType);
        }

        public static async Task InsertItem(object item, Type itemType)
        {
            if (AsyncConnection == null)
            {
                Connection = new SQLiteConnection(ConnectionString);
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            await AsyncConnection.InsertAsync(item, itemType);
            //ActivationContext.Trigger("OnDatabaseInsert",);
        }

        public static async Task UpdateItem(object item, Type itemType)
        {
            if (AsyncConnection == null)
            {
                Connection = new SQLiteConnection(ConnectionString);
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            await AsyncConnection.UpdateAsync(item, itemType);
        }

        public static async Task DeleteItem(object item)
        {
            if (AsyncConnection == null)
            {
                Connection = new SQLiteConnection(ConnectionString);
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            await AsyncConnection.DeleteAsync(item);
        }

        public static async Task<List<T>> GetItemsOfType<T>(Type itemType, Type ItemDAOType)
        {
            if (AsyncConnection == null)
            {
                Connection = new SQLiteConnection(ConnectionString);
                AsyncConnection = new SQLiteAsyncConnection(ConnectionString);
            }
            var tableMappings = AsyncConnection.TableMappings;
            var tableMapping = tableMappings.FirstOrDefault(x => x.MappedType == ItemDAOType);
            var tableName = tableMapping.TableName;
            var query = $"SELECT * FROM {tableName}";
            var args = new object[] { };
            var items = await AsyncConnection.QueryAsync(tableMapping,query, args);
            //convert the items to the correct type and return them
            List<T> returnItems = new List<T>();
            foreach (var item in items)
            {
                // Use dynamic casting along with the implicit operator
                T convertedItem = (T)(dynamic)item;
                returnItems.Add(convertedItem);
            }
            return returnItems;
        }


        public static async Task FillTeamserverFromDatabase()
        {
            try
            {
                Cred.CredList = await GetCreds();
                HistoryEvent.HistoryEventList = await GetHistoryEvents();
                //List<HttpManager> httpManagers = await GetHttpManagers();
                List<HttpManager> httpManagers = await GetItemsOfType<HttpManager>(typeof(HttpManager), typeof(HttpManager_DAO));
                ImanagerService.AddManagers(httpManagers);
                await managerService.StartManagersFromDB(httpManagers);


                ImanagerService.AddManagers(await GetTCPManagers());
                ImanagerService.AddManagers(await GetSMBManagers());
                DownloadFile.downloadFiles = await GetDownloadedFiles();
                UploadedFile.uploadedFileList = await GetUploadedFiles();
                IExtImplantService._extImplants.AddRange(await GetExtImplants());
                
                List<EncryptionKeys_DAO> encryptionKeys = await GetEncryptionKeys();
                foreach (EncryptionKeys_DAO key in encryptionKeys)
                {
                    if (key.ItemID == "UniversialMetadataKey")
                    {
                        Encryption.UniversialMetadataKey = key.Key;
                    }
                    else if (key.ItemID == "UniversialMessageKey")
                    {
                        Encryption.UniversialMessageKey = key.Key;
                    }
                    else if (key.ItemID == "UniversalTaskEncryptionKey")
                    {
                        Encryption.UniversalTaskEncryptionKey = key.Key;
                    }
                    else
                    {
                        bool keyAdded = Encryption.UniqueTaskEncryptionKey.TryAdd(key.ItemID, key.Key);
                        if(keyAdded == false)
                        {
                            Encryption.UniqueTaskEncryptionKey[key.ItemID] = key.Key;
                        }
                    }
                    
                }

                PivotProxy.PivotProxyList = await GetPivotProxies();
                ReconCenterEntity.ReconCenterEntityList = await GetReconCenterEntities();
                IOCFile.IOCFiles = await GetIOCFiles();
                Alias.savedAliases = await GetAliases();
                Webhook.ExistingWebhooks = await GetWebhooks();
                foreach (var iocFile in IOCFile.IOCFiles)
                {
                    await HardHatHub.AddIOCFile(iocFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        //create a function to get all the Cred item types from the db
        public static async Task<List<Cred>> GetCreds()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedCreds = await AsyncConnection.Table<Cred_DAO>().ToListAsync();
                //var returnedCreds = storedCreds.Select(x => (Cred)x);
                List<Cred> credList = new List<Cred>(storedCreds.Select(x => (Cred)x));

                //bool set to false because we just pulled from the db so we dont want to re add them
                return credList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all the historyevents from the database 
        public static async Task<List<HistoryEvent>> GetHistoryEvents()
        {
            try
            {
                if(AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedHistoryEvents = await AsyncConnection.Table<HistoryEvent_DAO>().ToListAsync();
                List<HistoryEvent> historyEventList = new List<HistoryEvent>(storedHistoryEvents.Select(x => (HistoryEvent)x));
                return historyEventList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null; 
            }
        }

        //a function to return all httpManagers from the database
        public static async Task<List<HttpManager>> GetHttpManagers()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedHttpManagers = AsyncConnection.Table<HttpManager_DAO>().ToListAsync().Result.Select(x => (HttpManager)x);
                List<HttpManager> httpManagerList = new List<HttpManager>(storedHttpManagers);
                return httpManagerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all the tcpManagers from the database
        public static async Task<List<TCPManager>> GetTCPManagers()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedTCPManagers = AsyncConnection.Table<TCPManager_DAO>().ToListAsync().Result.Select(x => (TCPManager)x);
                List<TCPManager> tcpManagerList = new List<TCPManager>(storedTCPManagers);
                return tcpManagerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all the smbManagers from the database
        public static async Task<List<SMBManager>> GetSMBManagers()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedSMBManagers = AsyncConnection.Table<SMBManager_DAO>().ToListAsync().Result.Select(x => (SMBManager)x);
                List<SMBManager> smbManagerList = new List<SMBManager>(storedSMBManagers);
                return smbManagerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all downloadedFiles from the database
        public static async Task<List<DownloadFile>> GetDownloadedFiles()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedDownloadedFiles = AsyncConnection.Table<DownloadFile_DAO>().ToListAsync().Result.Select(x => (DownloadFile)x);
                List<DownloadFile> downloadedFileList = new List<DownloadFile>(storedDownloadedFiles);
                return downloadedFileList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all the uploadedFiles from the database
        public static async Task<List<UploadedFile>> GetUploadedFiles()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedUploadedFiles = AsyncConnection.Table<UploadedFile_DAO>().ToListAsync().Result.Select(x => (UploadedFile)x);
                List<UploadedFile> uploadedFileList = new List<UploadedFile>(storedUploadedFiles);
                return uploadedFileList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to retrun all the EncryptionKeys from the database
        public static async Task<List<EncryptionKeys_DAO>> GetEncryptionKeys()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedEncryptionKeys = await AsyncConnection.Table<EncryptionKeys_DAO>().ToListAsync();
                return storedEncryptionKeys;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all the ExtImplantTaskResults from the database

        public static async Task<List<ExtImplant_Base>> GetExtImplants()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedExtImplants = AsyncConnection.Table<ExtImplant_DAO>().ToListAsync().Result.Select(x => (ExtImplant_Base)x);
                List<ExtImplant_Base> extimplantList = new List<ExtImplant_Base>(storedExtImplants);
                Console.WriteLine($"restored {extimplantList.Count} implants from the database");
                IExtImplantService.ImplantNumber = extimplantList.Count;
                return extimplantList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
        public static async Task<ExtImplant_Base> GetExtImplant(string Id)
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedExtImplant = AsyncConnection.Table<ExtImplant_DAO>().Where(x => x.id == Id).ToListAsync().Result.Select(x => (ExtImplant_Base)x).FirstOrDefault();
                return storedExtImplant;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public static async Task<List<ExtImplantTask_Base>> GetExtImplantTasks()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedExtImplantTasks = AsyncConnection.Table<ExtImplantTask_DAO>().ToListAsync().Result.Select(x => (ExtImplantTask_Base)x);
                List<ExtImplantTask_Base> extimplantTaskList = new List<ExtImplantTask_Base>(storedExtImplantTasks);
                return extimplantTaskList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
        
        public static async Task<List<ExtImplantTaskResult_Base>> GetExtImplantTaskResults()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedExtImplantTaskResults = AsyncConnection.Table<ExtImplantTaskResult_DAO>().ToListAsync().Result.Select(x => (ExtImplantTaskResult_Base)x);
                List<ExtImplantTaskResult_Base> extimplantTaskResultList = new List<ExtImplantTaskResult_Base>(storedExtImplantTaskResults);
                return extimplantTaskResultList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all the PivotProxy from the database
        public static async Task<List<PivotProxy>> GetPivotProxies()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedPivotProxies = AsyncConnection.Table<PivotProxy_DAO>().ToListAsync().Result.Select(x => (PivotProxy)x);
                List<PivotProxy> pivotProxyList = new List<PivotProxy>(storedPivotProxies);
                return pivotProxyList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all the ReconCenterEntities from the database
        public static async Task<List<ReconCenterEntity>> GetReconCenterEntities()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedReconCenterEntities = AsyncConnection.Table<ReconCenterEntity_DAO>().ToListAsync().Result.Select(x => (ReconCenterEntity)x);
                List<ReconCenterEntity> reconCenterEntityList = new List<ReconCenterEntity>(storedReconCenterEntities);
                return reconCenterEntityList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
        
        //function to return all the IOCFiles from the database
        public static async Task<List<IOCFile>> GetIOCFiles()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedIOCFiles = AsyncConnection.Table<IOCFIle_DAO>().ToListAsync().Result.Select(x => (IOCFile)x);
                List<IOCFile> iocFileList = new List<IOCFile>(storedIOCFiles);
                return iocFileList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all of the Alias objects from the database
        public static async Task<List<Alias>> GetAliases()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedAliases = AsyncConnection.Table<Alias_DAO>().ToListAsync().Result.Select(x => (Alias)x);
                List<Alias> aliasList = new List<Alias>(storedAliases);
                return aliasList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all the CompiledImplant objects from the database
        public static async Task<List<CompiledImplant>> GetCompiledImplants()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedCompiledImplants = AsyncConnection.Table<CompiledImplant_DAO>().ToListAsync().Result.Select(x => (CompiledImplant)x);
                List<CompiledImplant> compiledImplantList = new List<CompiledImplant>(storedCompiledImplants);
                return compiledImplantList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        //a function to return all webhooks from the database
        public static async Task<List<Webhook>> GetWebhooks()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedWebhooks = AsyncConnection.Table<Webhooks_DAO>().ToListAsync().Result.Select(x => (Webhook)x);
                List<Webhook> webhookList = new List<Webhook>(storedWebhooks);
                return webhookList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

    }
}
