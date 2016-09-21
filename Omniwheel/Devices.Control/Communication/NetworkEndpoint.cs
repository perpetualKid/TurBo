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
    public class NetworkEndpoint : CommunicationComponentBase
    {
        private Dictionary<DataFormat, ChannelBase> channels;

        public NetworkEndpoint(int port): base("TCP")
        {
            channels = new Dictionary<DataFormat, ChannelBase>();
        }

        private void Server_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void ComponentHelp()
        {
            throw new NotImplementedException();
        }

        public override void ProcessCommand(ControllableComponent sender, string[] commands)
        {
            throw new NotImplementedException();
        }

        public async Task AddChannel(int port, DataFormat format)
        {
            channels.Add(format, await SocketServer.AddChannel(port, format).ConfigureAwait(false));
        }

        public override async Task Send(object data)
        {
            throw new NotImplementedException();
        }
    }
}
