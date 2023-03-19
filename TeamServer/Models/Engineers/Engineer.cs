using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

// Model for Engineer defines class, constructors, and other needed functions 
namespace TeamServer.Models
{
    public class Engineer
    {
		//class memeber properties and varables 
		public EngineerMetadata engineerMetadata { get; set; }  //added metadata from class loads all properties from there , having no setter makes this read only so it can only be set in this line nad the constructor and not changed anywhere else.

        public string ConnectionType { get; set;}
		public string ManagerName { get; set;}
		public string ExternalAddress { get; set; }
        public DateTime LastSeen { get; set; } //private set because we dont ant any other class to set it
        public string Status { get; set; }
        //public int Sleep { get; set; }

        public readonly ConcurrentQueue<EngineerTask> _pendingTasks = new();
        private readonly List<EngineerTaskResult> _taskResults = new();
		public static Dictionary<string, List<EngineerTask>> previousTasks = new();
        public static Dictionary<string, ConcurrentQueue<EngineerTaskResult>> taskQueueDic = new();

        public Engineer(EngineerMetadata metadata) // makes constructor and includes metadata on creation
		{
			engineerMetadata = metadata;
		}

		public Engineer() { }

		public void CheckIn()
		{
			LastSeen = DateTime.Now;
			Status = "Active";
		}

		public IEnumerable<EngineerTask> GetPendingTasks()
		{
			List<EngineerTask> tasks = new();

			while (_pendingTasks.TryDequeue(out var task))
			{
				tasks.Add(task);
			}
			return tasks;
		}
		
        
		public bool QueueTask(EngineerTask task)
		{ 
			_pendingTasks.Enqueue(task);
			return true;
		}

		public EngineerTaskResult GetTaskResult(string taskId)
		{
			return GetTaskResults().FirstOrDefault(r => r.Id.Equals(taskId));
		}

		public IEnumerable<EngineerTaskResult> GetTaskResults()
		{
			IEnumerable<EngineerTaskResult> taskresults = _taskResults;
			return taskresults;
		}

		public EngineerTaskResult DeQueueTaskResults(string taskid)
		{
            taskQueueDic[taskid].TryDequeue(out var taskResult);
            return taskResult;
        }

		public void AddTaskResults(IEnumerable<EngineerTaskResult> results)
		{
			//if _taskResults contains an item with the same id as the result, then append the result field and update the status 
			foreach (var result in results)
			{
                var existing = _taskResults.FirstOrDefault(r => r.Id.Equals(result.Id));
				if (existing != null)
				{
					existing.Status = result.Status;
					existing.Result = existing.Result.Concat(result.Result).ToArray();
				}
				else
				{
					_taskResults.Add(result);
				}
			}
		}
	}
}
