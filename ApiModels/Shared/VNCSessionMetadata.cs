using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ApiModels.Shared
{
    public class VNCSessionMetadata
    {
        //session id of the vnc window
        public string SessionID { get; set; }
        
        //user who started the session
        public string startingUser { get; set; }
        
        // bool to filter input to only the starting user 
        public bool FilterInput { get; set; }
        
        //bool for sessionStarted
        public bool IsSessionRunning { get; set; }
        
        //last datetime in utc that the session was interacted with
        public DateTime LastInteraction { get; set; }
        
        //date time of last heartbeat request (view interaction) -> these are sent every sleep cycle or 100ms if sleep is 0
        public DateTime LastHeartbeatRequest { get; set; }
        
        //date time of last heartbeat response (view interaction) -> these are sent every sleep cycle or 100ms if sleep is 0
        public DateTime LastHeartbeatResponse { get; set; }

        public double ScreenWidth { get; set; }
        public double ScreenHeight { get; set; }
    }
}
