using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Shared
{
    public enum VncInteractionEvent
    {
        View,
        MouseClick,
        MouseMove,
        KeySend,
        clipboard,
        clipboardPaste,
    }
}
