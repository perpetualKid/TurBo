using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;


namespace Common.Communication.Channels
{
    public class JsonChannel : ChannelBase
    {
        private const int bufferSize = 512;
        private DataReaderLoadOperation loadOperation;
        private SemaphoreSlim streamAccess;

        private MemoryStream memoryStream;
        private long streamReadPosition;
        private long streamWritePosition;

        public JsonChannel(SocketObject socket) : base(socket, DataFormat.Json)
        {
            streamAccess = new SemaphoreSlim(1);
            memoryStream = new MemoryStream();
            this.OnMessageReceived += socketObject.Instance_OnMessageReceived;
            this.ConnectionStatus = ConnectionStatus.Disconnected;
        }

        public override Task Close()
        {
            throw new NotImplementedException();
        }

        public override Task Send(object data)
        {
            throw new NotImplementedException();
        }

        internal override async void BindAsync(StreamSocket socketStream)
        {
            this.ConnectionStatus = ConnectionStatus.Connecting;
            this.streamSocket = socketStream;
            try
            {
                cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(socketObject.CancellationTokenSource.Token);
                using (DataReader dataReader = new DataReader(socketStream.InputStream))
                {
                    CancellationToken cancellationToken = cancellationTokenSource.Token;
                    //setup
                    cancellationToken.ThrowIfCancellationRequested();
                    dataReader.InputStreamOptions = InputStreamOptions.Partial;
                    this.ConnectionStatus = ConnectionStatus.Connected;

                    ////Send a Hello message across
                    //await Parse("HELLO" + Environment.NewLine).ConfigureAwait(false);

                    loadOperation = dataReader.LoadAsync(bufferSize);
                    uint bytesAvailable = await loadOperation.AsTask(cancellationToken).ConfigureAwait(false);
                    while (bytesAvailable > 0 && loadOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                    {
                        await streamAccess.WaitAsync().ConfigureAwait(false);
                        if (streamWritePosition == streamReadPosition)
                        {
                            streamReadPosition = 0;
                            streamWritePosition = 0;
                            memoryStream.SetLength(0);
                        }
                        memoryStream.Position = streamWritePosition;
                        byte[] buffer = dataReader.ReadBuffer(bytesAvailable).ToArray();
                        await memoryStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                        streamWritePosition = memoryStream.Position;
                        streamAccess.Release();

                        await Parse().ConfigureAwait(false);
                        bytesRead += bytesAvailable;
                        loadOperation = dataReader.LoadAsync(bufferSize);
                        bytesAvailable = await loadOperation.AsTask(cancellationToken).ConfigureAwait(false);
                    }
                    dataReader.DetachBuffer();
                    dataReader.DetachStream();
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception exception)
            {
                socketObject.ConnectionStatus = ConnectionStatus.Failed;
                Debug.WriteLine(string.Format("Error receiving data: {0}", exception.Message));
            }
            this.ConnectionStatus = ConnectionStatus.Disconnected;
            this.OnMessageReceived -= socketObject.Instance_OnMessageReceived;
            //JsonTextReader jsonReader = new JsonTextReader(new StreamReader(socketStream.InputStream.AsStreamForRead()));
            //jsonReader.SupportMultipleContent = true;
            //jsonReader.CloseInput = false;

            //while (true)
            //{
            //    //await jsonReadStream.FlushAsync();
            //    //jsonReadStream.Position = readPosition;
            //    //                using (JsonTextReader jsonReader = new JsonTextReader(new StreamReader(jsonReadStream.GetInputStreamAt(0).AsStreamForRead())))
            //    {
            //        //jsonReader.SupportMultipleContent = true;
            //        //jsonReader.CloseInput = false;
            //        var serializer = new JsonSerializer();
            //        //if (!reader.Read() )//|| reader.TokenType != JsonToken.StartArray)
            //        //    throw new Exception("Expected start of array in the deserialized json string");
            //        try
            //        {
            //            while (jsonReader.Read())
            //            {
            //                if (jsonReader.TokenType == Newtonsoft.Json.JsonToken.StartObject)
            //                {
            //                    var item = serializer.Deserialize(jsonReader);
            //                    PublishMessageReceived(this, new JsonMessageArgs(JsonObject.Parse(item.ToString())));
            //                }
            //                //readPosition = (int)jsonReadStream.Position;
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            Debug.WriteLine(ex.Message);
            //        }
            //    }
            //}
            //this.ConnectionStatus = ConnectionStatus.Disconnected;
            //this.OnMessageReceived -= socketObject.Instance_OnMessageReceived;
        }

        private async Task Parse()
        {
            using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8, true, bufferSize, true))
            {
                StringBuilder builder = new StringBuilder();
                await streamAccess.WaitAsync().ConfigureAwait(false);

                JsonStreamParser parser = new JsonStreamParser(reader, streamReadPosition);

                memoryStream.Position = streamReadPosition;
                foreach (JsonObject item in parser) // reader.GetJsonObjects())
                {
                    Debug.WriteLine(item.Stringify());
                }

                //string[] message = reader.GetJsonObjects().ToArray();
                //if (message.Length > 0)
                //    PublishMessageReceived(this, new StringMessageArgs(message));

                streamReadPosition = parser.ReadPosition;//memoryStream.Position;
                streamAccess.Release();
            }
        }

    }
}
