using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication;
using Common.Communication.Channels;
using Devices.Control.Base;

namespace Devices.Control.Communication
{
    public class NetworkListener : CommunicationComponentBase
    {
        private int port;
        private DataFormat dataFormat = DataFormat.StringText;
        private SocketObject instance;

        public NetworkListener(int port): base("TCP" + port.ToString())
        {
            this.port = port;
        }

        public NetworkListener(int port, DataFormat dataFormat) : this(port)
        {
            this.dataFormat = dataFormat;
        }

        public override async Task InitializeDefaults()
        {
            this.instance = await SocketServer.RegisterChannelListener(port, dataFormat);
            instance.OnMessageReceived += Server_OnMessageReceived;
        }

        public override async Task Close(ControllableComponent sender)
        {
            await instance.Close().ConfigureAwait(false);
        }
        private async void Server_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            await HandleInput(new ChannelHolder(sender as ChannelBase), (e as StringMessageReceivedEventArgs).Message);
        }

        public override async Task ComponentHelp(ControllableComponent sender)
        {
            await Task.CompletedTask;
        }

        public override async Task ProcessCommand(ControllableComponent sender, string[] commands)
        {
            await Task.CompletedTask;
        }

        public override async Task Send(ControllableComponent sender, object data)
        {
            if (sender is ChannelHolder)
                await (sender as ChannelHolder).Channel.Send(data);
            else
            {
                //
            }
        }
    }
}
