﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Plugin_BaseClasses;
using HardHatCore.ApiModels.Shared;

namespace HardHatCore.TeamServer.Plugin_Interfaces.Ext_Implants
{
    public interface IExtImplant : IExtImplantData
    {
        public ExtImplantMetadata_Base Metadata { get; set; }
        public string ImplantType { get; set; }
        public int Number { get; set; }
        public string Note { get; set; }
        public string ConnectionType { get; set; }
        public string ExternalAddress { get; set; }
        public DateTime LastSeen { get; set; }
        public DateTime FirstSeen { get; set; }
        public string Status { get; set; }


        public Task CheckIn();
        public Task<IEnumerable<ExtImplantTask_Base>> GetPendingTasks();
        public Task<IEnumerable<AssetNotification>> GetAssetNotifications();
        public Task<bool> QueueTask(ExtImplantTask_Base task);
        public Task<ExtImplantTaskResult_Base> GetTaskResult(string taskId);
        public Task<IEnumerable<ExtImplantTaskResult_Base>> GetTaskResults();
        public Task AddTaskResults(IEnumerable<ExtImplantTaskResult_Base> results);
        public Task AddTaskResult(ExtImplantTaskResult_Base result);
    }

    public interface IExtImplantData : IPluginMetadata
    {
    }
}
