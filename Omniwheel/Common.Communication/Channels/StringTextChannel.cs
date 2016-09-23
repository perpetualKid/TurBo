using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Communication.Channels;
using Nito.AsyncEx;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Common.Communication.Channels
{
    public class StringTextChannel : ChannelBase
    {
        private Queue<string> queue;
        private const uint bufferSize = 512;
        private static readonly char[] lineBreak = { '\r', '\n', '\0' };

        private static List<StringTextChannel> instances = new List<StringTextChannel>();

        #region static
        public static async Task EstablishConnection(SocketObject socketObject, StreamSocket socketStream)
        {
            StringTextChannel instance = new StringTextChannel(socketObject);
            instances.Add(instance);
            instance.OnMessageReceived += socketObject.Instance_OnMessageReceived;
            DataReaderLoadOperation loadOperation;
            instance.streamSocket = socketStream;
            try
            {
                using (DataReader dataReader = new DataReader(socketStream.InputStream))
                {
                    CancellationToken cancellationToken = socketObject.CancellationTokenSource.Token;
                    //setup
                    lock (socketObject.CancellationTokenSource)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        dataReader.InputStreamOptions = InputStreamOptions.Partial;
                    }

                    loadOperation = dataReader.LoadAsync(bufferSize);
                    uint bytesAvailable = await loadOperation.AsTask(cancellationToken).ConfigureAwait(false);
                    while (bytesAvailable > 0 && loadOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                    {
                        instance.queue.Enqueue(dataReader.ReadString(bytesAvailable));
                        instance.dataReadEvent.Set();
                        instance.bytesRead += bytesAvailable;
                        loadOperation = dataReader.LoadAsync(bufferSize);
                        bytesAvailable = await loadOperation.AsTask(cancellationToken).ConfigureAwait(false);
                    }
                    dataReader.DetachBuffer();
                    dataReader.DetachStream();
                }
            }
            catch (Exception exception)
            {
                socketObject.ConnectionStatus = ConnectionStatus.Failed;
                Debug.WriteLine(string.Format("Error receiving data: {0}", exception.Message));
            }
            if (null != instance)
            {
                instances.Remove(instance);
                instance.OnMessageReceived -= socketObject.Instance_OnMessageReceived;
            }
        }
        #endregion

        #region instance
        public StringTextChannel(SocketObject socket) : base(socket, DataFormat.StringText)
        {
            queue = new Queue<string>();
        }


        public override async Task Listening(StreamSocket socket)
        {
            DataReaderLoadOperation loadOperation;
            this.streamSocket = socket;
            try
            {
                socketObject.ConnectionStatus = ConnectionStatus.Connected;
                using (DataReader dataReader = new DataReader(socket.InputStream))
                {
                    CancellationToken cancellationToken = socketObject.CancellationTokenSource.Token;
                    //setup
                    lock (socketObject.CancellationTokenSource)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        dataReader.InputStreamOptions = InputStreamOptions.Partial;
                    }

                    loadOperation = dataReader.LoadAsync(bufferSize);
                    uint bytesAvailable =  await loadOperation.AsTask(cancellationToken).ConfigureAwait(false);
                    while (bytesAvailable > 0 && loadOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                    {
                        queue.Enqueue(dataReader.ReadString(bytesAvailable));
                        dataReadEvent.Set();
                        bytesRead += bytesAvailable;
                        loadOperation = dataReader.LoadAsync(bufferSize);
                        bytesAvailable = await loadOperation.AsTask(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception exception)
            {
                socketObject.ConnectionStatus = ConnectionStatus.Failed;
                Debug.WriteLine(string.Format("Error receiving data: {0}", exception.Message));
            }
        }

        protected override async Task ParseData()
        {
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                await dataReadEvent.WaitAsync();
                while (queue.Count > 0)
                {
                    string buffer = queue.Dequeue();
                    string[] lines = buffer.Split(lineBreak);
                        foreach (string line in lines)
                        {
                        if (string.IsNullOrWhiteSpace(line) && builder.Length > 0)
                        {
                            PublishMessageReceived(this, new StringMessageReceivedEventArgs(builder.ToString()));
                            builder.Clear();
                        }
                        else
                        {
                            builder.Append(line);
                        }
                    }
                }
            }
        }

        public override async Task Send(object data)
        {
            using (DataWriter writer = new DataWriter(streamSocket.OutputStream))
            {
                string text = data as string;
                if (!string.IsNullOrEmpty(text))
                {
                    bytesWritten += writer.WriteString(text);
                    char last = text[text.Length - 1];
                    if (last != '\0' && last != '\r' && last != '\n')
                        bytesWritten += writer.WriteString(Environment.NewLine);
                    await writer.StoreAsync();
                    await writer.FlushAsync();

                    writer.DetachBuffer();
                    writer.DetachStream();
                }
            }
        }
        #endregion
    }
}