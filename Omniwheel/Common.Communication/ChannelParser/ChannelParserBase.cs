using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication.Channels;
using Windows.Networking.Sockets;

namespace Common.Communication.ChannelParser
{
    public abstract class ChannelParserBase
    {
        protected SocketObject socket;

        public ChannelParserBase(SocketObject socket)
        {
            this.socket = socket;
        }

        public abstract void ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args);

        public abstract void ParseData();

    }
}
