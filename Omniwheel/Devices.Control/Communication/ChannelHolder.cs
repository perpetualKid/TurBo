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
        public ChannelHolder(string componentName) : base(componentName)
        {
        }

        public ChannelBase Channel { get; set; }

        public CommunicationComponentBase Endpoint { get; set; }

        public override void ComponentHelp()
        {
            throw new NotImplementedException();
        }

        public override void ProcessCommand(ControllableComponent sender, string[] commands)
        {
            throw new NotImplementedException();
        }
    }
}
