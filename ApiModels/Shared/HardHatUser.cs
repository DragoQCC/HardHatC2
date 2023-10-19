using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ApiModels.Shared
{
    public class HardHatUser
    {
        public string SignalRClientId { get; set; } //track machine to send notifs to 
        public string Username { get; set; } //track user to send notifs to
        public List<string> Roles { get; set; } //track if the user is an admin or not
    }
}
