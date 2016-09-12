using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Common.Communication.Channels
{
    public class SocketClient: SocketObject
    {
        private HostName hostName;
        private StreamSocket streamSocket;
        private DataWriter dataWriter;
        private DataReader dataReader;


        public async Task Connect(string remoteServer, string remotePort)
        {
            try
            {
                ConnectionStatus = ConnectionStatus.Connecting;
                hostName = new HostName(remoteServer);
                streamSocket = new StreamSocket();
                streamSocket.Control.NoDelay = true;
                await streamSocket.ConnectAsync(hostName, remotePort);
                ConnectionStatus = ConnectionStatus.Connected;
                dataWriter = new DataWriter(streamSocket.OutputStream);
            }
            catch (Exception e)
            {
                ConnectionStatus = ConnectionStatus.Failed;
                //todo:report errors via event to be consumed by UI thread
            }
        }

        public async Task Disconnect()
        {
            if (dataWriter != null)
            {
                await dataWriter.FlushAsync();
                dataWriter.DetachStream();
                dataWriter.Dispose();
                dataWriter = null;
            }
            if (streamSocket != null)
            {
                await streamSocket.CancelIOAsync();
                streamSocket.Dispose();
                streamSocket = null;
            }
            ConnectionStatus = ConnectionStatus.Disconnected;
        }

        public async Task SendMessage(string message)
        {
            //            _writer.WriteUInt32(_writer.MeasureString(message));
            dataWriter.WriteString(message);
            await dataWriter.StoreAsync();
            await dataWriter.FlushAsync();
        }
    }
}
