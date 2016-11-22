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
        public static async Task<ChannelBase> BindChannelAsync(DataFormat dataFormat, SocketObject host, StreamSocket socketStream)
        {
            ChannelBase channel = null;
            switch (dataFormat)
            {
                case DataFormat.StringText:
                    channel = new StringTextChannel(host);
                    channel.BindAsync(socketStream);
                    break;
                case DataFormat.Json:
                    channel = new JsonChannel(host);
                    channel.BindAsync(socketStream);
                    break;
                default:
                    await Task.CompletedTask.ConfigureAwait(false);
                    break;
            }
            return channel;
        }
    }
}
