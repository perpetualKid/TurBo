using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication.Channels;
using Devices.Control.Base;

namespace Devices.Control.Communication
{
    public class ChannelHolder: Controllable
    {
        public ChannelHolder(ChannelBase channel) : base("ChannelHolder")
        {
            this.Channel = channel;
        }

        public ChannelBase Channel { get; }

        public CommunicationComponentBase Endpoint { get; set; }

        public override async Task ComponentHelp(Controllable sender)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public override async Task ProcessCommand(Controllable sender, string[] commands)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
