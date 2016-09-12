using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{

    public enum DataFormat
    {
        Unknown,
        String,
        LineDelimited,
        Json,
        FrameSize,
    }

    public enum ConnectionStatus
    {
        Failed = -1,
        Disconnected = 0,
        Connecting = 1,
        Listening = 2,
        Connected = 3,
    }
}
