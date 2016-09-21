using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devices.Control.Base;

namespace Devices.Control.Communication
{
    public abstract class CommunicationComponentBase : ControllableComponent
    {
        public CommunicationComponentBase(string componentName) : base(componentName)
        {
        }

        public abstract Task Send(object data);

    }
}
