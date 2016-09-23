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
        protected DataFormat dataFormat;

        private ConnectionStatus connectionStatus;

        #region public events
        public event EventHandler<ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;

        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        #endregion

        #region base
        public ChannelBase(SocketObject socket, DataFormat format)
        {
            this.socketObject = socket;
            this.dataFormat = format;
            dataReadEvent = new AsyncAutoResetEvent();
            parseTask = Task.Run(async () => await ParseData());
            parseTask.ConfigureAwait(false);
        }

        public abstract Task Listening(StreamSocket socket);

        internal abstract Task BindAsync(StreamSocket socket);

        protected abstract Task ParseData();

        public abstract Task Send(object data);
        #endregion

        #region public properties
        public StreamSocket StreamSocket { get { return this.streamSocket; } set { this.streamSocket = value; } }

        public DataFormat DataFormat { get { return this.dataFormat; } }

        public uint BytesWritten { get { return bytesWritten; } }

        public uint BytesRead { get { return bytesRead; } }

        protected virtual void PublishMessageReceived(ChannelBase sender, MessageReceivedEventArgs eventArgs)
        {   
            OnMessageReceived?.Invoke(sender, eventArgs);
        }

        public ConnectionStatus ConnectionStatus
        {
            get { return connectionStatus; }
            internal set
            {
                if (value != connectionStatus)
                {
                    connectionStatus = value;
                    OnConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs { Status = value });
                }
            }
        }


        #endregion
    }
}
