using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication.Channels
{
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        public ConnectionStatus Status;
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        object message;

        public MessageReceivedEventArgs(object item)
        {
            this.message = item;
        }

        public MessageReceivedEventArgs(string message)
        {
            this.message = message;
        }

        public string MessageAsString { get { return message.ToString(); } }

        public object Message { get { return this.message; } }

    }
}
