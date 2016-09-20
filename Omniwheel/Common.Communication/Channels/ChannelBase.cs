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
        protected uint bytesRead;
        protected uint bytesWritten;

        public event EventHandler<ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;


        #region base
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
        #endregion

        #region public properties
        public StreamSocket StreamSocket { get { return this.streamSocket; } set { this.streamSocket = value; } }
        public uint BytesWritten { get { return bytesWritten; } }

        public uint BytesRead { get { return bytesRead; } }

        protected virtual void PublishMessageReceived(MessageReceivedEventArgs eventArgs)
        {
            OnMessageReceived?.Invoke(this, eventArgs);
        }
        #endregion
    }
}
