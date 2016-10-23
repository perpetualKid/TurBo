using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Base.Communication
{
    public abstract class CommunicationControllable : Controllable
    {
        public CommunicationControllable(string componentName) : base(componentName)
        {
        }

        public abstract Task Send(MessageContainer data);

        public abstract Task Close(MessageContainer data);
    }
}
