using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

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

    public class JsonMessageReceivedEventArgs : MessageReceivedEventArgs
    {

        private JsonObject json;

        public JsonMessageReceivedEventArgs(JsonObject json)
        {
            this.json = json;
        }

        public JsonObject Json { get { return json; } }
    }

}
