using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication.Channels;

namespace Common.Communication.ChannelParser
{
    static class ChannelParserFactory
    {
        public static ChannelParserBase CreateChannelParser(SocketObject socket, DataFormat dataFormat)
        {
            switch(dataFormat)
            {
                case DataFormat.String:
                    return new StringParser(socket);
                        default:
                    return (ChannelParserBase)null;

            }
        }
    }
}
