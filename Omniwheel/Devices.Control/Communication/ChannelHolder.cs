using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Communication.Channels;
using Devices.Control.Base;

namespace Devices.Control.Communication
{
    public class ChannelHolder: ControllableComponent
    {
        public ChannelHolder(ChannelBase channel) : base("ChannelHolder")
        {
            this.Channel = channel;
        }

        public ChannelBase Channel { get; }

        public CommunicationComponentBase Endpoint { get; set; }

        public override async Task ComponentHelp(ControllableComponent sender)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public override async Task ProcessCommand(ControllableComponent sender, string[] commands)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }
    }
}
