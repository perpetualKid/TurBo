using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Communication.ChannelParser;
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
        private ChannelParserBase channelParser;
        private StreamSocketListener socketListener;


        #region instance fields

        //exposed to hook into new messages
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        #endregion

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

        static async Task AddListener(int port, DataFormat dataFormat)
        {
            SocketServer instance = Instance(port);
            await instance.AddListener(dataFormat);
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
        
        public async Task AddListener(DataFormat dataFormat)
        {
            if (socketListener != null)
                throw new InvalidOperationException("Only one Listner can be attached to this port.");
            try
            {
                this.ConnectionStatus = ConnectionStatus.Connecting;
                channelParser = ChannelParserFactory.CreateChannelParser(this, dataFormat);
                socketListener = new StreamSocketListener();
                socketListener.Control.NoDelay = true;
                socketListener.ConnectionReceived += channelParser.ConnectionReceived;
                await socketListener.BindServiceNameAsync(port.ToString()).AsTask().ConfigureAwait(false);
                this.ConnectionStatus = ConnectionStatus.Listening;

                await Task.Run(() => channelParser.ParseData());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                this.ConnectionStatus = ConnectionStatus.Failed;
            }
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
                socketListener.ConnectionReceived -= channelParser.ConnectionReceived;
                socketListener.Dispose();
                socketListener = null;
                ConnectionStatus = ConnectionStatus.Disconnected;
            }
        }
        #endregion


        private async void JsonConverter()
        {
            //JsonTextReader jsonReader = new JsonTextReader(new StreamReader(jsonReadStream));
            //jsonReader.SupportMultipleContent = true;
            //jsonReader.CloseInput = false;

            //int writePosition = 0;
            //int readPosition = 0;

            while (true)
            {
//                await jsonReadStream.FlushAsync();
//                jsonReadStream.Position = readPosition;
////                using (JsonTextReader jsonReader = new JsonTextReader(new StreamReader(jsonReadStream.GetInputStreamAt(0).AsStreamForRead())))
//                {
//                    //jsonReader.SupportMultipleContent = true;
//                    //jsonReader.CloseInput = false;
//                    var serializer = new JsonSerializer();
//                    //if (!reader.Read() )//|| reader.TokenType != JsonToken.StartArray)
//                    //    throw new Exception("Expected start of array in the deserialized json string");
//                    try
//                    {
//                        while (jsonReader.Read())
//                        {
//                            if (jsonReader.TokenType == JsonToken.StartObject)
//                            {
//                                var item = serializer.Deserialize(jsonReader);
//                                OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(item));
//                            }
//                            readPosition = (int)jsonReadStream.Position;
//                        }
//                    }
//                    catch(Exception ex)
//                    {
//                        Debug.WriteLine(ex.Message);
//                    }

//                }
            }

        }

        private async void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            uint bufferSize = 512;

            ConnectionStatus = ConnectionStatus.Connected;
            try
            {
                //using (var reader = new JsonTextReader(new StreamReader(args.Socket.InputStream.AsStreamForRead())))
                //{
                //    reader.SupportMultipleContent = true;
                //    var serializer = new JsonSerializer();
                //    //if (!reader.Read() )//|| reader.TokenType != JsonToken.StartArray)
                //    //    throw new Exception("Expected start of array in the deserialized json string");

                //    while (reader.Read())
                //    {
                //        if (reader.TokenType == JsonToken.StartObject)
                //        {
                //            var item = serializer.Deserialize(reader);
                //            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(item));
                //        }
                //    }


                //}

                //socket = args.Socket;
                //try
                //{
            }
            //        while (true)
            //    {
            //        ResetReadCancellationTokenSource();
            //        CancellationToken cancellationToken = readCancellationTokenSource.Token;
            //        // Don't start any IO if we canceled the task
            //        lock (readCancelLock)
            //        {
            //            cancellationToken.ThrowIfCancellationRequested();

            //            if (dataReader == null)
            //                dataReader = new DataReader(args.Socket.InputStream);
            //            dataReader.InputStreamOptions = InputStreamOptions.Partial;

            //            Windows.Storage.Streams.UnicodeEncoding encoding = dataReader.UnicodeEncoding;
            //        }

            //        uint bufferSize = 512;
            //        uint bytesRead = await dataReader.LoadAsync(bufferSize).AsTask(cancellationToken).ConfigureAwait(false);
            //        try
            //        {
            //            while (bytesRead > 0)
            //            {
            //                //byte[] buffer = new byte[bytesRead];
            //                //dataReader.ReadBytes(buffer);
            //                //queue.Enqueue(buffer);
            //                string text = dataReader.ReadString(bytesRead);
            //                queue.Enqueue(text);
            //                //await jsonReadStream.WriteAsync(buffer);
            //                //await jsonReadStream.FlushAsync();
            //                jsonReadEvent.Set();
            //                //if (bytesRead < bufferSize)
            //                //{
            //                //    await localStream.FlushAsync();
            //                //    TextReader reader = new StreamReader(localStream.GetInputStreamAt(0).AsStreamForRead());
            //                //    Debug.Write(reader.ReadToEnd());
            //                //    break;
            //                //}
            //                bytesRead = await dataReader.LoadAsync(512).AsTask(cancellationToken).ConfigureAwait(false) ;
            //            }
            //            if (bytesRead == 0)
            //                break;
            //        }
            //        catch(TaskCanceledException)
            //        {
            //            //
            //        }
            //        //    dataReader.ReadBytes(payloadSize);



            //        //    // Read the payload.

            //        //    int size = BitConverter.ToInt32(payloadSize, 0);

            //        //    byte[] payload = new byte[size];

            //        //    await dataReader.LoadAsync((uint)size);

            //        //    dataReader.ReadBytes(payload);// Create a task object to wait for data on the InputStream

            //        //}
            //        //        //// Launch the task and wait
            //        //        UInt32 bytesRead = await loadAsyncTask.ConfigureAwait(false);

            //        //        byte[] buffer = null;
            //        //        if (bytesRead > 0)
            //        //        {
            //        //            buffer = new byte[bytesRead];
            //        //            dataReader.ReadBytes(buffer);
            //        //            readBytesCounter += bytesRead;
            //        //            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs(buffer));
            //        //        }
            //        //        else
            //        //            Debug.WriteLine(String.Format("No bytes received"));


            //        //        //uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
            //        //        //if (sizeFieldCount != sizeof(uint))
            //        //        //{
            //        //        //    connectionStatus = ConnectionStatus.Disconnected;
            //        //        //    break;
            //        //        //}

            //        //        //uint bufferSize = reader.ReadUInt32();
            //        //        //uint actualSize = await reader.LoadAsync(bufferSize);
            //        //        //if (bufferSize != actualSize)
            //        //        //{
            //        //        //    connectionStatus = ConnectionStatus.Disconnected;
            //        //        //    break;
            //        //        //}
            //        //        //byte[] result = null;
            //        //        //if (bytesRead > 0)
            //        //        //{
            //        //        //    result = new byte[bytesRead];
            //        //        //    reader.ReadBytes(result); //return the bytes read
            //        //        //}
            //        //        //else
            //        //        //    Debug.WriteLine(String.Format("No bytes received"));
            //        //        ////ReadBuffer(actualSize);
            //        //    }
            //    }
            //}
            catch (Exception ex)
            {
                connectionStatus = ConnectionStatus.Failed;
                Debug.WriteLine(string.Format("Error receiving data: {0}", ex.Message));
                //if (dataReader != null)
                //{
                //    dataReader.DetachStream();
                //    dataReader.Dispose();
                //    dataReader = null;
                //}
            }
        }


    }
}
