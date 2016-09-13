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
        private readonly char[] lineBreak = { '\r', '\n', '\0' };

        public StringTextChannel(SocketObject socket) : base(socket)
        {
            queue = new Queue<string>();
        }

        public override async Task Listening(StreamSocket socket)
        {
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

                    uint bytesRead = await dataReader.LoadAsync(bufferSize).AsTask(cancellationToken).ConfigureAwait(false);
                    while (bytesRead > 0)
                    {
                        socketObject.BytesRead = bytesRead;
                        queue.Enqueue(dataReader.ReadString(bytesRead));
                        dataReadEvent.Set();
                        bytesRead = await dataReader.LoadAsync(bufferSize).AsTask(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception exception)
            {
                socketObject.ConnectionStatus = ConnectionStatus.Failed;
                Debug.WriteLine(string.Format("Error receiving data: {0}", exception.Message));
            }
        }

        public override async Task ParseData()
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
                                socketObject.PublishMessageReceived(new StringMessageReceivedEventArgs(builder.ToString()));
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

        public override async Task SendData(object data)
        {
            using (DataWriter writer = new DataWriter(streamSocket.OutputStream))
            {
                string text = data as string;
                if (!string.IsNullOrEmpty(text))
                {
                    socketObject.BytesWritten = writer.WriteString(text);
                    char last = text[text.Length - 1];
                    if (last != '\0' && last != '\r' && last != '\n')
                        socketObject.BytesWritten = writer.WriteString(Environment.NewLine);
                    await writer.StoreAsync();
                    await writer.FlushAsync();

                    writer.DetachBuffer();
                    writer.DetachStream();
                }
            }
        }

    }
}