using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardHatCore.ApiModels.Shared.TaskResultTypes
{
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
}
