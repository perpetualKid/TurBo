using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace Common.Communication.Channels
{
    public abstract class ChannelBase
    {
        protected SocketObject socketObject;
        protected StreamSocket streamSocket;
        protected CancellationTokenSource cancellationTokenSource;

        protected uint bytesRead;
        protected uint bytesWritten;
        private readonly DataFormat dataFormat;
        private readonly Guid sessionId;

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
            this.sessionId = Guid.NewGuid();
        }

        internal abstract void BindAsync(StreamSocket socketStream);

        public abstract Task Send(object data);

        public abstract Task Close();
        #endregion

        #region public properties
        public StreamSocket StreamSocket { get { return this.streamSocket; } set { this.streamSocket = value; } }

        public DataFormat DataFormat { get { return this.dataFormat; } }

        public Guid SessionId { get { return this.sessionId; } }
             

        public uint BytesWritten { get { return bytesWritten; } }

        public uint BytesRead { get { return bytesRead; } }

        protected virtual void PublishMessageReceived(ChannelBase sender, MessageReceivedEventArgs eventArgs)
        {
            eventArgs.SessionId = this.sessionId;
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
                    OnConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs { SessionId = this.sessionId, Status = value });
                }
            }
        }


        #endregion
    }
}
