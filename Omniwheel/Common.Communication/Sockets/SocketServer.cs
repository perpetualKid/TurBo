using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Communication.Channels;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Common.Communication.Channels
{
    public class SocketServer: SocketObject
    {
        private static Dictionary<int, SocketServer> activeSockets = new Dictionary<int, SocketServer>();

        private int port;
        private StreamSocketListener socketListener;

        #region static
        public static SocketServer Instance(int port)
        {
            if (!activeSockets.ContainsKey(port))
            {
                lock (activeSockets)
                {
                    SocketServer instance = new SocketServer(port);
                    activeSockets.Add(port, instance);
                }
            }
            return activeSockets[port];
        }

        public static async Task<ChannelBase> AddChannel(int port, DataFormat dataFormat)
        {
            SocketServer instance = Instance(port);
            return await instance.AddChannel(dataFormat);
        }
        #endregion

        #region Instance
        #region .ctor
        private SocketServer(int port)
        {
            this.port = port;
            cancellationTokenSource = new CancellationTokenSource();
        }
        #endregion

        #region public properties
        public int Port { get { return this.port; } }
        #endregion
        
        public async Task<ChannelBase> AddChannel(DataFormat dataFormat)
        {
            if (socketListener != null)
                throw new InvalidOperationException("Only one Listner can be attached to this port.");
            try
            {
                this.ConnectionStatus = ConnectionStatus.Connecting;
                channel = ChannelFactory.CreateChannel(this, dataFormat);
                socketListener = new StreamSocketListener();
                socketListener.Control.NoDelay = true;
                socketListener.ConnectionReceived += async (streamSocketListener, streamSocketListenerConnectionReceivedEventArgs) => await channel.Listening(streamSocketListenerConnectionReceivedEventArgs.Socket).ConfigureAwait(false);
                await socketListener.BindServiceNameAsync(port.ToString()).AsTask().ConfigureAwait(false);
                this.ConnectionStatus = ConnectionStatus.Listening;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                this.ConnectionStatus = ConnectionStatus.Failed;
            }
            return channel;
        }


        /// <summary>
        /// Disposes of the TCP socket and sets it to null. 
        /// Returns false if no socket was listening.
        /// </summary>
        public async Task StopListening()
        {
            if (socketListener != null)
            {
                CancelSocketTask();
                await socketListener.CancelIOAsync();
                socketListener.Dispose();
                socketListener = null;
                ConnectionStatus = ConnectionStatus.Disconnected;
            }
        }
        #endregion

        public override async Task Send(object data)
        {
            await channel.Send(data);
        }

        //private async void JsonConverter()
        //{
        //    JsonTextReader jsonReader = new JsonTextReader(new StreamReader(jsonReadStream));
        //    jsonReader.SupportMultipleContent = true;
        //    jsonReader.CloseInput = false;

        //    int writePosition = 0;
        //    int readPosition = 0;

        //    while (true)
        //    {
        //        await jsonReadStream.FlushAsync();
        //        jsonReadStream.Position = readPosition;
        //        //                using (JsonTextReader jsonReader = new JsonTextReader(new StreamReader(jsonReadStream.GetInputStreamAt(0).AsStreamForRead())))
        //        {
        //            //jsonReader.SupportMultipleContent = true;
        //            //jsonReader.CloseInput = false;
        //            var serializer = new JsonSerializer();
        //            //if (!reader.Read() )//|| reader.TokenType != JsonToken.StartArray)
        //            //    throw new Exception("Expected start of array in the deserialized json string");
        //            try
        //            {
        //                while (jsonReader.Read())
        //                {
        //                    if (jsonReader.TokenType == JsonToken.StartObject)
        //                    {
        //                        var item = serializer.Deserialize(jsonReader);
        //                        OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(item));
        //                    }
        //                    readPosition = (int)jsonReadStream.Position;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.WriteLine(ex.Message);
        //            }

        //        }
        //    }

        //}

    }
}
