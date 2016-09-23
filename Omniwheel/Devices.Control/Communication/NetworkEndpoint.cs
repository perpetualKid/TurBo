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

        public override async Task InitializeComponent()
        {
            await SocketServer.AddChannelListener(8027, DataFormat.StringText);
            SocketServer.OnMessageReceived += Server_OnMessageReceived;
            //channel.OnMessageReceived += Server_OnMessageReceived;
            //channels.Add(channel.DataFormat, channel);
            //ChannelBase channel = await SocketServer.AddChannel(8027, DataFormat.StringText);
            //channel.OnMessageReceived += Server_OnMessageReceived;
            //channels.Add(channel.DataFormat, channel);
        }

        private async void Server_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            await HandleInput(new ChannelHolder(sender as ChannelBase), (e as StringMessageReceivedEventArgs).Message);
        }

        public override void ComponentHelp()
        {
            throw new NotImplementedException();
        }

        public override void ProcessCommand(ControllableComponent sender, string[] commands)
        {
            throw new NotImplementedException();
        }

        //public async Task AddChannel(int port, DataFormat format)
        //{
        //    //channels.Add(format, await SocketServer.AddChannel(port, format).ConfigureAwait(false));
        //}

        public override async Task Send(ControllableComponent sender, object data)
        {
            if (sender is ChannelHolder)
                await (sender as ChannelHolder).Channel.Send(data);
            else
            {
                List<Task> sendTasks = new List<Task>();
                foreach (ChannelBase channel in channels.Values)
                {
                    sendTasks.Add(channel.Send(data));
                }
                await Task.WhenAll(sendTasks).ConfigureAwait(false);
            }
        }
    }
}
