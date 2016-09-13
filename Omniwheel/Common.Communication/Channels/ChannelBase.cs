using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication.Channels;
using Nito.AsyncEx;
using Windows.Networking.Sockets;

namespace Common.Communication.Channels
{
    public abstract class ChannelBase
    {
        protected SocketObject socketObject;
        protected StreamSocket streamSocket;

        protected AsyncAutoResetEvent dataReadEvent;
        private Task parseTask;


        public ChannelBase(SocketObject socket)
        {
            this.socketObject = socket;
            dataReadEvent = new AsyncAutoResetEvent();
            parseTask = Task.Run(async () => await ParseData());
            parseTask.ConfigureAwait(false);
        }

        public abstract Task Listening(StreamSocket socket);

        public abstract Task ParseData();

        public abstract Task SendData(object data);

        public StreamSocket StreamSocket { get { return this.streamSocket; } set { this.streamSocket = value; } }
    }
}
