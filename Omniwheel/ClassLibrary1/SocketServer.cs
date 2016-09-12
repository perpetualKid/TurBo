using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WindowsIoTSocketServer
{
    public static class SocketServer
    {
        //exposed to hook into server status changes
        public static event EventHandler<ConnectionStatusChangedEventArgs> ConnectStatusChanged;

        //exposed to hook into new messages
        public static event EventHandler<MessageSentEventArgs> NewMessageReady;

        private const string Port = "8027";

        private static StreamSocketListener socketListener;
        private static ConnectionStatus connectionStatus;

        public static ConnectionStatus ConnectionStatus
        {
            get { return connectionStatus; }
            set
            {
                if (value != connectionStatus)
                {
                    connectionStatus = value;
                    var args = new ConnectionStatusChangedEventArgs { Status = connectionStatus };
                    var handler = ConnectStatusChanged;
                    handler?.Invoke(ConnectionStatus, args);
                }
            }
        }

        private static string message;

        public static string Message
        {
            get { return message; }
            set
            {
                if (value != message)
                {
                    message = value;
                    var args = new MessageSentEventArgs { Message = message };
                    var handler = NewMessageReady;
                    handler?.Invoke(Message, args);
                }
            }
        }

        public static async void StartListener()
        {
            try
            {
                ConnectionStatus = ConnectionStatus.Connecting;
                socketListener = new StreamSocketListener();
                socketListener.ConnectionReceived += OnConnection;
                await socketListener.BindServiceNameAsync(Port);
                ConnectionStatus = ConnectionStatus.Listening;
            }
            catch (Exception e)
            {
                ConnectionStatus = ConnectionStatus.Failed;
            }
        }

        private static async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            ConnectionStatus = ConnectionStatus.Connected;
            DataReader reader = new DataReader(args.Socket.InputStream);
            try
            {
                while (true)
                {
                    uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
                    if (sizeFieldCount != sizeof(uint))
                    {
                        return;
                    }

                    uint stringLength = reader.ReadUInt32();
                    uint actualStringLength = await reader.LoadAsync(stringLength);
                    if (stringLength != actualStringLength)
                    {
                        return;
                    }

                    Message = reader.ReadString(actualStringLength);
                }
            }
            catch (Exception e)
            {
                ConnectionStatus = ConnectionStatus.Failed;
                //TODO:send a connection status message with error
            }
        }
    }

    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        public ConnectionStatus Status;
    }

    public class MessageSentEventArgs : EventArgs
    {
        public string Message;
    }

    public enum ConnectionStatus
    {
        Idle = 0,
        Connecting = 1,
        Listening = 2,
        Connected = 3,
        Failed = 99
    }
}
