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
        private ChannelBase channel;


        public SocketClient()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<ChannelBase> Connect(string remoteServer, string remotePort, DataFormat dataFormat)
        {
            try
            {
                ConnectionStatus = ConnectionStatus.Connecting;
                hostName = new HostName(remoteServer);
                streamSocket = new StreamSocket();
                streamSocket.Control.NoDelay = true;
                await streamSocket.ConnectAsync(hostName, remotePort);
                ConnectionStatus = ConnectionStatus.Connected;

                channel = await ChannelFactory.BindChannelAsync(dataFormat, this, streamSocket).ConfigureAwait(false);

                //channel = ChannelFactory.BindChannel(format, this);
                //channel.StreamSocket = streamSocket;
                //Task task = Task.Run(async () => await channel.Listening(streamSocket));
            }
            catch (Exception exception)
            {
                ConnectionStatus = ConnectionStatus.Failed;
                Debug.WriteLine(string.Format("Error receiving data: {0}", exception.Message));
            }
            return channel;
        }

        public async Task Disconnect()
        {

            await Task.Run( () => CancelSocketTask());
            ConnectionStatus = ConnectionStatus.Disconnected;
        }

        public override async Task Send(Guid sessionId, object data)
        {
            await channel.Send(data);
        }

        public override async Task Close()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public override async Task CloseSession(Guid sessionId)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
