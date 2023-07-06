using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TeamServer.Services;
//using DynamicEngLoading;

// Model for Engineer defines class, constructors, and other needed functions 
namespace TeamServer.Models
{
    public class Engineer
    {
		//class memeber properties and varables 
		public EngineerMetadata engineerMetadata { get; set; }  //added metadata from class loads all properties from there , having no setter makes this read only so it can only be set in this line nad the constructor and not changed anywhere else.
        public int Number { get; set; }
		public string Note { get; set; }
        public string ConnectionType { get; set;}
		public string ManagerName { get; set;}
		public string ExternalAddress { get; set; }
        public DateTime LastSeen { get; set; } 
        public DateTime FirstSeen { get; set; }
        public string Status { get; set; }

        public readonly ConcurrentQueue<EngineerTask> _pendingTasks = new();
        public List<EngineerTaskResult> _taskResults = new();
		public static Dictionary<string, List<EngineerTask>> previousTasks = new();
        public static Dictionary<string, ConcurrentQueue<EngineerTaskResult>> taskQueueDic = new();

        public Engineer(EngineerMetadata metadata) // makes constructor and includes metadata on creation
		{
			engineerMetadata = metadata;
			//get the last engineer and increment the number by 1 and set the FirstSeen to now in UTC 
			Number = EngineerService._engineers.Count > 0 ? EngineerService._engineers.Last().Number + 1 : 1;
			FirstSeen = DateTime.UtcNow;
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

        public List<string> GetTaskIds()
        {
	        //get all the task ids that match the values in the list
	        var taskIds = _taskResults.Select(r => r.Id).ToList();
			return taskIds;
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
