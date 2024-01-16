using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Plugin_Interfaces;
using HardHatCore.ApiModels.Shared;
using HardHatCore.TeamServer.Models.Dbstorage;
using HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants;
using HardHatCore.TeamServer.Services;

namespace HardHatCore.TeamServer.Plugin_BaseClasses
{
    public class ExtImplant_Base : IExtImplant
    {
        public string Name { get; } = "Default";
        public string Description { get; } = "Default Implant class";
        public ExtImplantMetadata_Base Metadata { get; set; }
        //this should be the name of the implant such as Engineer, Constructor, Rustineer, etc.
        public  string ImplantType { get; set; }
        //this number should be the checkin number of the implant, this is used as a filter in the client UI for example if this is the 6th implant to checkin it will be number 6
        public  int Number { get; set; }
        public  string Note { get; set; }
        public  string ConnectionType { get; set; }
        public  string ExternalAddress { get; set; }
        public  DateTime LastSeen { get; set; }
        public  DateTime FirstSeen { get; set; }
        public string Status { get; set; }
        
        
        public ConcurrentQueue<ExtImplantTask_Base> pendingTasks  = new();
        public ConcurrentDictionary<string, byte[]> TaskResultDataChunks = new();
        public ConcurrentQueue<AssetNotification> assetNotifications = new();
        
        //this is used to create a new instance of the implant, and should be coded into the implant itself to return a useable instance of the implant    
        public virtual ExtImplant_Base GetNewIExtImplant(IExtImplantMetadata imp_meta)
        {
            ExtImplant_Base implant_Base = new();
            implant_Base.Metadata = (ExtImplantMetadata_Base)imp_meta;
            implant_Base.Number = IExtImplantService.ImplantNumber;
            implant_Base.FirstSeen = DateTime.UtcNow;
            return implant_Base;
        }

        public virtual async Task AddTaskResults(IEnumerable<ExtImplantTaskResult_Base> results)
        {
           foreach(var taskres in results)
            {
                await AddTaskResult(taskres);
            }
        }

        public virtual async Task AddTaskResult(ExtImplantTaskResult_Base result)
        {
            // put the result in the database
            if(DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();  
            }
            ExtImplantTaskResult_Base existing = await DatabaseService.AsyncConnection.Table<ExtImplantTaskResult_DAO>().FirstOrDefaultAsync(x => x.TaskId == result.Id);
            if (existing != null)
            {
                //concat if the command is not download, for download the result is already the full file and we dont want to concat it
                existing.Result = existing.Command.Equals("download", StringComparison.CurrentCultureIgnoreCase) is false ? existing.Result.Concat(result.Result).ToArray() : result.Result;
            }
            await DatabaseService.AsyncConnection.InsertAsync((ExtImplantTaskResult_DAO)result);
        }

        public virtual async Task AddTask(ExtImplantTask_Base impTask)
        {
            // put the result in the database
            if (DatabaseService.AsyncConnection == null)
            {
                DatabaseService.ConnectDb();
            }
            DatabaseService.AsyncConnection.InsertAsync((ExtImplantTask_DAO)impTask);
        }

        //this is just to update the info for the implant to let the server know it is still alive, the implant service will handle the rest
        public virtual async Task CheckIn()
        {
            LastSeen = DateTime.UtcNow;
            Status = "Active";
        }

        public virtual async Task<IEnumerable<ExtImplantTask_Base>> GetPendingTasks()
        {
            List<ExtImplantTask_Base> outboundTasks = new();
            while (pendingTasks.TryDequeue(out ExtImplantTask_Base task))
            {
                outboundTasks.Add(task);
            }
            return outboundTasks;
        }

        public virtual async Task<IEnumerable<AssetNotification>> GetAssetNotifications()
        {
            List<AssetNotification> notificationList = new();
            while (this.assetNotifications.TryDequeue(out AssetNotification assetNotification))
            {
                notificationList.Add(assetNotification);
            }
            return notificationList;
        }

        public virtual async Task<ExtImplantTaskResult_Base> GetTaskResult(string taskId)
        {
            //check the database for the task result
            if(DatabaseService.AsyncConnection is null)
            {
                DatabaseService.ConnectDb();
            }
            var returnedItem = await DatabaseService.AsyncConnection.Table<ExtImplantTaskResult_DAO>().FirstOrDefaultAsync(x => x.TaskId == taskId);
            return returnedItem ?? null;
        }

        public virtual async Task<IEnumerable<ExtImplantTaskResult_Base>> GetTaskResults()
        {
            return DatabaseService.GetExtImplantTaskResults().Result.Where(x => x.ImplantId == Metadata.Id).ToList();
        }

        public virtual async Task<IEnumerable<ExtImplantTask_Base>> GetTasks()
        {
            return DatabaseService.GetExtImplantTasks().Result.Where(x => x.ImplantId == Metadata.Id);
        }

        public virtual async Task<ExtImplantTask_Base> GetTask(string taskId)
        {
            try
            {
                if (DatabaseService.AsyncConnection == null)
                {
                    DatabaseService.ConnectDb();
                }
                var storedExtImplantTask = DatabaseService.AsyncConnection.Table<ExtImplantTask_DAO>().ToListAsync().Result.FirstOrDefault(x => x.Id == taskId);
                return storedExtImplantTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public virtual async Task<bool> RemoveTask(ExtImplantTask_Base taskToRemove)
        {
            return DatabaseService.AsyncConnection.DeleteAsync((ExtImplantTask_DAO)taskToRemove).Result == 1;
        }

        public virtual async Task<bool> QueueTask(ExtImplantTask_Base task)
        {
            try
            {
                await AddTask(task);
                pendingTasks.Enqueue(task);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
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
