using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication.Channels;
using Windows.Networking.Sockets;

namespace Common.Communication.Channels
{
    static class ChannelFactory
    {
        public static ChannelBase BindChannel(DataFormat dataFormat, SocketObject socket)
        {
            switch(dataFormat)
            {
                case DataFormat.StringText:
                    return new StringTextChannel(socket);
                        default:
                    return default(ChannelBase);
            }
        }

        public static async Task BindChannel(DataFormat dataFormat, SocketObject host, StreamSocket socketStream)
        {
            switch (dataFormat)
            {
                case DataFormat.StringText:
                    await StringTextChannel.EstablishConnection(host, socketStream);
                    break;
                default:
                    await Task.CompletedTask;
                    break;
            }
        }

    }
}
