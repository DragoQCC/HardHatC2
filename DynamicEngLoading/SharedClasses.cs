using System.Collections.Generic;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace DynamicEngLoading
{
    public interface IEngineerCommand
    {
        string Name { get; }
        Task Execute(EngineerTask task);
    }
    [Serializable]
    public abstract class EngineerCommand : IEngineerCommand
    {
        public abstract string Name { get; }

        public virtual bool IsHidden { get; set; } = false;

        public abstract Task Execute(EngineerTask task);
    }

    [Serializable]
    public class EngineerTask
    {
        public string Id { get; set; }

        public string Command { get; set; }

        public Dictionary<string, string> Arguments { get; set; }

        public byte[] File { get; set; }

        public bool IsBlocking { get; set; }


        [NonSerialized]
        public CancellationToken cancelToken = new CancellationToken();

        public EngineerTask() { }

        public EngineerTask(string Id, string Command, Dictionary<string, string> Arguments, byte[] File, bool IsBlocking)
        {
            this.Id = Id;
            this.Command = Command;
            this.Arguments = Arguments;
            this.File = File;
            this.IsBlocking = IsBlocking;
        }
    }


    [Serializable]
    public class EngineerTaskResult
    {
        public string Id { get; set; }

        public string Command { get; set; }

        public byte[] Result { get; set; }

        public bool IsHidden { get; set; }

        public string ImplantId { get; set; }

        public EngTaskStatus Status { get; set; }

        public TaskResponseType ResponseType { get; set; }
    }
    public enum EngTaskStatus
    {
        Running = 2,
        Complete = 3,
        FailedWithWarnings = 4,
        CompleteWithErrors = 5,
        Failed = 6,
        Cancelled = 7
    }
    public enum TaskResponseType
    {
        None,
        String,
        FileSystemItem,
        ProcessItem,
        TokenStoreItem,
        FilePart,
        DataChunk,
        EditFile,
        VncInteractionEvent,
    }

    public class FilePart
    {
        public int Type { get; set; } // 1 = file part, 2 = end of file
        public int Length { get; set; } // Size of data in bytes
        public byte[] Data { get; set; } // Actual data
    }

    public class TokenStoreItem
    {
        public int Index { get; set; }
        public string Username { get; set; }
        public int PID { get; set; }
        public string SID { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ProcessItem
    {
        public string ProcessName { get; set; }
        public string ProcessPath { get; set; }
        public string Owner { get; set; }
        public int ProcessId { get; set; }
        public int ProcessParentId { get; set; }
        public int SessionId { get; set; }
        public string Arch { get; set; }
    }
    
    [Serializable]
    public class FileSystemItem
    {
        public string Name { get; set; } = "";
        public long Length { get; set; } = 0;
        public string Owner { get; set; } = "";
        public long ChildItemCount { get; set; } = 0;
        public DateTime CreationTimeUtc { get; set; } = new DateTime();
        public DateTime LastAccessTimeUtc { get; set; } = new DateTime();
        public DateTime LastWriteTimeUtc { get; set; } = new DateTime();
        public List<ACL> ACLs { get; set; } = new List<ACL>();
    }

    [Serializable]
    public class ACL
    {
        public string IdentityRef { get; set; } = "";
        public string AccessControlType { get; set; } = "";
        public string FileSystemRights { get; set; } = "";
        public bool IsInherited { get; set; }
    }

    public class DataChunk
    {
        public int Type { get; set; } // 1 is a part, 2 marks we hit the last of the byte[] 
        public int Position { get; set; } // Position of this chunk in the file
        public int Length { get; set; } // Size of data in bytes
        public byte[] Data { get; set; } // Actual data
        public TaskResponseType RealResponseType { get; set; } //this is the og response type since the task would have its reponse type updated to DataChunk
    }

    public class EditFile
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public bool CanEdit { get; set; }
    }

    public class VncInteractionResponse
    {
        public string SessionID { get; set; }
        public byte[] ScreenContent { get; set; }
        public string ClipboardContent { get; set; }
        public VncInteractionEvent InteractionEvent { get; set; }
        public double ScreenWidth { get; set; }
        public double ScreenHeight { get; set; }
        public double MouseX { get; set; }
        public double MouseY { get; set; }
    }

    public enum  VncInteractionEvent
    {
        View,
        MouseClick,
        MouseMove,
        KeySend,
        clipboard,
        clipboardPaste,
    }

    [Serializable]
    public class AssetNotification
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        //Id of the Implant / Asset the Notification is for / from
        public string AssetId { get; set; }
        //The name of the Notification to trigger on
        public string NotificationName { get; set; }
        public int? NotificationType { get; set; }

        //Collection of string / byte[] pairs to be used as the data sent with the notification such as important Ids, Traffic, etc.
        public Dictionary<string, byte[]> NotificationData { get; set; } = new Dictionary<string, byte[]>();

        //This is set by deserializing data from an Asset and is used to determine if the notification should be forwarded to the client, ex. A Task Cancel notification
        public bool ForwardToClient { get; set; } = false;
    }
}
