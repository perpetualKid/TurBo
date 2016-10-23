using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Base;
using Common.Base.Communication;
using Common.Communication;
using Common.Communication.Channels;

namespace Devices.Control.Communication
{
    public class NetworkListener : CommunicationControllable
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

        public override async Task Close(MessageContainer data)
        {
            await instance.CloseSession(data.SessionId).ConfigureAwait(false);
        }

        private async void Server_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //            await HandleInput(new ChannelHolder(sender as ChannelBase), (e as StringMessageReceivedEventArgs).Message);
            //dynamic eventArgs = e;
            //await HandleInput(new ChannelHolder(sender as ChannelBase), eventArgs);
            await HandleInput(new MessageContainer(e.SessionId, this, (e as StringMessageArgs).Parameters));
        }

        public override async Task ComponentHelp(MessageContainer data)
        {
            await Task.CompletedTask;
        }


        public override async Task ProcessCommand(MessageContainer data)
        {
            await Task.CompletedTask;
        }

        public override async Task Send(MessageContainer data)
        {
            await instance.Send(data.SessionId, data.GetText());
        }
    }
}
