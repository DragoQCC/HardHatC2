using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TeamServer.Models;
using TeamServer.Models.Database;
using TeamServer.Models.Dbstorage;
using TeamServer.Models.Extras;
using TeamServer.Models.Managers;
using System.Threading;
using TeamServer.Utilities;
using ApiModels.Shared;
//using DynamicEngLoading;

namespace TeamServer.Services
{
    public class DatabaseService
    {
        public static string ConnectionString = null;
        public static SQLiteAsyncConnection AsyncConnection = null;
        public static SQLiteConnection Connection = null;
        private static List<Type> dbItemTypes = new List<Type> { typeof(Cred_DAO), typeof(DownloadFile_DAO), typeof(EncryptionKeys_DAO), typeof(Engineer_DAO), typeof(EngineerTaskResult_DAO), typeof(HistoryEvent_DAO),
                                                        typeof(HttpManager_DAO), typeof(PivotProxy_DAO),typeof(ReconCenterEntity_DAO), typeof(SMBManager_DAO), typeof(TCPManager_DAO), typeof(UploadedFile_DAO),typeof(EngineerTask_DAO),
                                                typeof(IOCFIle_DAO),typeof(CompiledImplant_DAO),typeof(Alias_DAO) };


        //create a function to init the database file, location string, and setup tables 
        public static void Init()
        {
            char allPlatformPathSeperator = Path.DirectorySeparatorChar;
            // find the Engineer cs file and load it to a string so we can update it and then run the compiler function on it
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

        public static async Task FillTeamserverFromDatabase()
        {
            try
            {
                Cred.CredList = await GetCreds();
                HistoryEvent.HistoryEventList = await GetHistoryEvents();
                List<Httpmanager> httpManagers = await GetHttpManagers();
                managerService._managers.AddRange(httpManagers);

                //make an http post to the managers controller calling the AddManagersFromDB and pass in the httpManagers list
                // var request = new RestRequest("/managers/addDB", Method.Post);
                // request.AddBody(httpManagers);
                // _ = await Startup.client.PostAsync<IActionResult>(request);
                await managerService.StartManagersFromDB(httpManagers);


                managerService._managers.AddRange(await GetTCPManagers());
                managerService._managers.AddRange(await GetSMBManagers());
                DownloadFile.downloadFiles = await GetDownloadedFiles();
                UploadedFile.uploadedFileList = await GetUploadedFiles();
                EngineerService._engineers.AddRange(await GetEngineers());
                List<EngineerTaskResult> taskResults = await GetEngineerTaskResults();
                List<EngineerTask> taskHeader = await GetEngineerTasks();
                // for each engineer in EngineerService._engineers add the task results that match the engineer id
                if (EngineerService._engineers.Count > 0)
                {
                    foreach (Engineer engineer in EngineerService._engineers)
                    {
                        engineer.AddTaskResults(taskResults.Where(x => x.EngineerId == engineer.engineerMetadata.Id));
                    }
                }
                //for each taskHeader find the matching Id in the taskResults, and use the engineerId to add the Header to the correct engineer in Engineer.PreviousTasks
                if (taskHeader != null && taskHeader.Count > 0)
                {
                    foreach (EngineerTask task in taskHeader)
                    {
                        EngineerTaskResult result = taskResults.Where(x => x.Id == task.Id).FirstOrDefault();
                        if (result != null)
                        {
                            Engineer engineer = EngineerService._engineers.Where(x => x.engineerMetadata.Id == result.EngineerId).FirstOrDefault();
                            if (engineer != null)
                            {
                                if (Engineer.previousTasks.ContainsKey(engineer.engineerMetadata.Id))
                                {
                                    Engineer.previousTasks[engineer.engineerMetadata.Id].Add(task);
                                }
                                else
                                {
                                    Engineer.previousTasks.Add(engineer.engineerMetadata.Id, new List<EngineerTask>() { task });
                                }
                            }
                        }
                    }
                }
                
                List<EncryptionKeys_DAO> encryptionKeys = await GetEncryptionKeys();
                foreach (EncryptionKeys_DAO key in encryptionKeys)
                {
                    if (key.ItemID == "UniversialMetadataKey")
                    {
                        Encryption.UniversialMetadataKey = key.Key;
                    }
                    else if (key.ItemID == "UniversialMessagePathKey")
                    {
                        Encryption.UniversialMessagePathKey = key.Key;
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
        public static async Task<List<Httpmanager>> GetHttpManagers()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedHttpManagers = AsyncConnection.Table<HttpManager_DAO>().ToListAsync().Result.Select(x => (Httpmanager)x);
                List<Httpmanager> httpManagerList = new List<Httpmanager>(storedHttpManagers);
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
        public static async Task<List<SMBmanager>> GetSMBManagers()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedSMBManagers = AsyncConnection.Table<SMBManager_DAO>().ToListAsync().Result.Select(x => (SMBmanager)x);
                List<SMBmanager> smbManagerList = new List<SMBmanager>(storedSMBManagers);
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

        //a function to return all the EngineerTaskResults from the database

        public static async Task<List<Engineer>> GetEngineers()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedEngineers = AsyncConnection.Table<Engineer_DAO>().ToListAsync().Result.Select(x => (Engineer)x);
                List<Engineer> engineerList = new List<Engineer>(storedEngineers);
                return engineerList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public static async Task<List<EngineerTask>> GetEngineerTasks()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedEngineerTasks = AsyncConnection.Table<EngineerTask_DAO>().ToListAsync().Result.Select(x => (EngineerTask)x);
                List<EngineerTask> engineerTaskList = new List<EngineerTask>(storedEngineerTasks);
                return engineerTaskList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
        
        public static async Task<List<EngineerTaskResult>> GetEngineerTaskResults()
        {
            try
            {
                if (AsyncConnection == null)
                {
                    ConnectDb();
                }
                var storedEngineerTaskResults = AsyncConnection.Table<EngineerTaskResult_DAO>().ToListAsync().Result.Select(x => (EngineerTaskResult)x);
                List<EngineerTaskResult> engineerTaskResultList = new List<EngineerTaskResult>(storedEngineerTaskResults);
                return engineerTaskResultList;
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

    }
}
