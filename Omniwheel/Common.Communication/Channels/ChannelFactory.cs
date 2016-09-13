using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication.Channels;

namespace Common.Communication.Channels
{
    static class ChannelFactory
    {
        public static ChannelBase CreateChannel(SocketObject socket, DataFormat dataFormat)
        {
            switch(dataFormat)
            {
                case DataFormat.StringText:
                    return new StringTextChannel(socket);
                        default:
                    return (ChannelBase)null;
            }
        }
    }
}
