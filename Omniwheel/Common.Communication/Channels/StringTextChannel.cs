using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Linq;
using System.Collections;

namespace Common.Communication.Channels
{
    public class StringTextChannel : ChannelBase
    {
        private const int bufferSize = 512;
        private DataReaderLoadOperation loadOperation;
        private SemaphoreSlim streamAccess;

        private MemoryStream memoryStream;
        private long streamReadPosition;
        private long streamWritePosition;


        #region instance
        public StringTextChannel(SocketObject socket) : base(socket, DataFormat.StringText)
        {
            streamAccess = new SemaphoreSlim(1);
            memoryStream = new MemoryStream();
            this.OnMessageReceived += socketObject.Instance_OnMessageReceived;
            this.ConnectionStatus = ConnectionStatus.Disconnected;
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

                    //Send a Hello message across
                    await Parse("HELLO" + Environment.NewLine).ConfigureAwait(false);

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
        }

        public override async Task Listening(StreamSocket socketStream)
        {
            this.streamSocket = socketStream;
            try
            {
                socketObject.ConnectionStatus = ConnectionStatus.Connected;
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
                    uint bytesAvailable =  await loadOperation.AsTask(cancellationToken).ConfigureAwait(false);
                    while (bytesAvailable > 0 && loadOperation.Status == Windows.Foundation.AsyncStatus.Completed)
                    {
                        //queue.Enqueue(dataReader.ReadString(bytesAvailable));
                        //dataReadEvent.Set();
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

        private async Task Parse()
        {
            using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8, true, bufferSize, true))
            {
                StringBuilder builder = new StringBuilder();
                await streamAccess.WaitAsync();

                memoryStream.Position = streamReadPosition;
                string[] message = reader.GetTokens().ToArray();
                if (message.Length > 0)
                    PublishMessageReceived(this, new StringMessageArgs(message));

                streamReadPosition = memoryStream.Position;
                streamAccess.Release();
            }
        }

        private async Task Parse(string data)
        {
            string[] message = data.GetTokens().ToArray();
            if (message.Length > 0)
                PublishMessageReceived(this, new StringMessageArgs(message));
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public override async Task Send(object data)
        {
            IList textData = data as IList;
            if (null == textData || textData.Count == 0)
            {
                throw new FormatException("Data is invalid or empty and cannot be send as text.");
            }
            using (DataWriter writer = new DataWriter(streamSocket.OutputStream))
            {
                foreach (var line in textData)
                {
                    bytesWritten += writer.WriteString(FormatSendData(line));
                    await writer.StoreAsync();
                }
                await writer.FlushAsync();
                writer.DetachBuffer();
                writer.DetachStream();
            }
        }

        private static string FormatSendData(object data)
        {
            StringBuilder result = new StringBuilder();
            result.Append(data?.ToString());
            char last = result[result.Length - 1];
            if (last != '\0' && last != '\r' && last != '\n')
                result.AppendLine();
            return result.ToString();
        }

        public override async Task Close()
        {
            if (null != loadOperation)
            {
                await Task.Run(() =>
                {
                    cancellationTokenSource.Cancel();
                    loadOperation.Cancel();
                    loadOperation.Close();
                }
                );
            }
        }
        #endregion
    }
}