using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Communication.Channels;
using Windows.Data.Json;

namespace TurBoControl.Controller
{
    public class DeviceConnection
    {
        private SocketClient socketClient;
        private static DeviceConnection instance;

        public event EventHandler<ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;

        private DeviceConnection()
        {
            this.socketClient = new SocketClient();
            socketClient.OnConnectionStatusChanged += SocketClient_OnConnectionStatusChanged;
        }
        #region event handling
        private void SocketClient_OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            OnConnectionStatusChanged?.Invoke(this, e);
        }

        private void SocketClient_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine((e as JsonMessageArgs).Json.ToString());
            //TODO need to correlate back to the requestor
        }
        #endregion

        public static DeviceConnection Instance
        {
            get
            {
                if (null == instance)
                    instance = new DeviceConnection();
                return instance;
            }
        }

        public ConnectionStatus ConnectionStatus { get { return socketClient.ConnectionStatus; } }

        public async Task Connect(string host, string port)
        {
            if (socketClient.ConnectionStatus != ConnectionStatus.Connected)
            {
                ChannelBase channel = await socketClient.Connect(host, port, DataFormat.Json);
                if (channel?.ConnectionStatus == ConnectionStatus.Connected)
                    channel.OnMessageReceived += SocketClient_OnMessageReceived;
            }
        }

        public async Task Send(JsonObject data)
        {
            if (socketClient.ConnectionStatus == ConnectionStatus.Connected)
            {
                await socketClient.Send(Guid.Empty, data);
            }
        }
    }
}
