using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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

        public SocketClient()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task Connect(string remoteServer, string remotePort, DataFormat format)
        {
            try
            {
                ConnectionStatus = ConnectionStatus.Connecting;
                hostName = new HostName(remoteServer);
                streamSocket = new StreamSocket();
                streamSocket.Control.NoDelay = true;
                await streamSocket.ConnectAsync(hostName, remotePort);
                ConnectionStatus = ConnectionStatus.Connected;
                channel = ChannelFactory.CreateChannel(this, format);
                channel.StreamSocket = streamSocket;
                Task task = Task.Run(async () => await channel.Listening(streamSocket));
            }
            catch (Exception exception)
            {
                ConnectionStatus = ConnectionStatus.Failed;
                Debug.WriteLine(string.Format("Error receiving data: {0}", exception.Message));
            }
        }

        public async Task Disconnect()
        {

            await Task.Run( () => CancelSocketTask());
            ConnectionStatus = ConnectionStatus.Disconnected;
        }

        public override async Task Send(object data)
        {
            await channel.SendData(data);
        }
    }
}
