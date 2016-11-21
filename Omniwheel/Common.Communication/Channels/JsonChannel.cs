using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Common.Communication.Channels
{
    public class JsonChannel : ChannelBase
    {
        private Queue<string> queue;
        private const uint bufferSize = 512;

        public JsonChannel(SocketObject socket) : base(socket, DataFormat.Json)
        {
            queue = new Queue<string>();
            this.OnMessageReceived += socketObject.Instance_OnMessageReceived;
            this.ConnectionStatus = ConnectionStatus.Disconnected;
        }

        public override Task Close()
        {
            throw new NotImplementedException();
        }

        public override Task Listening(StreamSocket socket)
        {
            throw new NotImplementedException();
        }

        public override Task Send(object data)
        {
            throw new NotImplementedException();
        }

        internal override async void BindAsync(StreamSocket socketStream)
        {
            JsonTextReader jsonReader = new JsonTextReader(new StreamReader(socketStream.InputStream.AsStreamForRead()));
            jsonReader.SupportMultipleContent = true;
            jsonReader.CloseInput = false;

            this.ConnectionStatus = ConnectionStatus.Connecting;
            this.streamSocket = socketStream;

            await Task.CompletedTask.ConfigureAwait(false);
            while (true)
            {
                //await jsonReadStream.FlushAsync();
                //jsonReadStream.Position = readPosition;
                //                using (JsonTextReader jsonReader = new JsonTextReader(new StreamReader(jsonReadStream.GetInputStreamAt(0).AsStreamForRead())))
                {
                    //jsonReader.SupportMultipleContent = true;
                    //jsonReader.CloseInput = false;
                    var serializer = new JsonSerializer();
                    //if (!reader.Read() )//|| reader.TokenType != JsonToken.StartArray)
                    //    throw new Exception("Expected start of array in the deserialized json string");
                    try
                    {
                        while (jsonReader.Read())
                        {
                            if (jsonReader.TokenType == JsonToken.StartObject)
                            {
                                var item = serializer.Deserialize(jsonReader);
                                PublishMessageReceived(this, new JsonMessageArgs(JsonObject.Parse(item.ToString())));
                            }
                            //readPosition = (int)jsonReadStream.Position;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            this.ConnectionStatus = ConnectionStatus.Disconnected;
            this.OnMessageReceived -= socketObject.Instance_OnMessageReceived;
        }
    }
}
