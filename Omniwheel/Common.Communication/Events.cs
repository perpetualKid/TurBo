using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Common.Communication
{
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        public Guid SessionId;

        public ConnectionStatus Status;
    }

    public abstract class MessageReceivedEventArgs : EventArgs
    {
        public Guid SessionId;
    }

    public class DataReceivedEventArgs: EventArgs
    {

    }

    public class StringMessageArgs: MessageReceivedEventArgs
    {

        private string[] parameters;
        private string message;

        public StringMessageArgs(string[] lines)
        {
            this.parameters = lines;
        }

        public StringMessageArgs(string message)
        {
            this.message = message;

            //StringTextParser parser = new StringTextParser(message);
            //foreach(string command in parser)
            //{
            //    Parameters.Add(command, null);
            //}

            //string[] commands = message.Split(':', ' ');
            //foreach (string item in commands)
            //{
            //    string[] nameValuePair = item.Split('=');
            //    if (nameValuePair.Length > 1)
            //        Parameters.Add(nameValuePair[0], nameValuePair[0]);
            //    else
            //        Parameters.Add(item, null);
            //}
        }

        public string Message { get { return message; } }

        public string[] Parameters { get { return parameters; } }
    }

    public class JsonMessageArgs : MessageReceivedEventArgs
    {

        private JsonObject json;

        public JsonMessageArgs(JsonObject json)
        {
            this.json = json;
        }

        public JsonObject Json { get { return json; } }
    }

}
