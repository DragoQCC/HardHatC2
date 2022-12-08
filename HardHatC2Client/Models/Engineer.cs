using HardHatC2Client.Pages;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace HardHatC2Client.Models
{
    public class Engineer : INotifyPropertyChanged
    {
        public EngineerMetadata engineerMetadata { get; set;}

        private  DateTime _lastSeen;
        private string _lastSeenString;
        private string _status; 
        
        public string Id { get; set; }
        public string Address { get; set; }
        public string Hostname { get; set; }
        public string Username { get; set; }
        public string ProcessName { get; set; }
        public int ProcessId { get; set; }
        public string Integrity { get; set; }
        public string Arch { get; set; }
        public DateTime LastSeen { get => _lastSeen; set { _lastSeen = value; OnTimerUpdated?.Invoke(); } }
        public string LastSeenTimer { get => _lastSeenString; set { _lastSeenString = value; OnTimerUpdated?.Invoke(); } }

        public string ExternalAddress { get; set; }
        public string ConnectionType { get; set; }
        public string ManagerName { get; set; }

        public int Sleep { get; set; }
        public string Status { get => _status; set => SetProperty(ref _status, value); }


        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action OnTimerUpdated;
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) //compares the old value to the new one and if its new it fires the OnPropertyChanged Event
        {
            if (Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Init()
        {
            Id = engineerMetadata.Id;
            Address = engineerMetadata.Address;
            Hostname = engineerMetadata.Hostname;
            Username = engineerMetadata.Username;
            ProcessName = engineerMetadata.ProcessName;
            ProcessId = engineerMetadata.ProcessId;
            Integrity = engineerMetadata.Integrity;
            Arch = engineerMetadata.Arch;
            ManagerName = engineerMetadata.ManagerName;
            Sleep = engineerMetadata.Sleep;
        }
    }
}
