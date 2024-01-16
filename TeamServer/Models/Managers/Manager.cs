using System;
using System.Threading.Tasks;
using HardHatCore.ApiModels.Shared;

/* models are just normal classes, that can hold properties and functions we want for an object.
 * can be called from controllers, or services as needed
 * cant be used with depenecy injection like a service can
 */
namespace HardHatCore.TeamServer.Models
{
    public abstract class Manager
    {
        public string Id { get; set; } = Guid.NewGuid().ToString(); // id for the Manager
        public abstract string Name { get; set; }

        public abstract ManagerType Type { get; set; } // enum of values http,https,tcp,smb

        public DateTime CreationTime { get; set; } = DateTime.UtcNow;


        public abstract Task Start();
        public abstract void Stop();
    }
}
