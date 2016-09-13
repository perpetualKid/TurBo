using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Communication
{
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        public ConnectionStatus Status;
    }

    public abstract class MessageReceivedEventArgs : EventArgs
    {

    }

    public class StringMessageReceivedEventArgs: MessageReceivedEventArgs
    {

        private string message;

        public StringMessageReceivedEventArgs(string message)
        {
            this.message = message;
        }

        public string Message { get { return message; } }
    }
}
