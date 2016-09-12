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

namespace Common.Communication.ChannelParser
{
    public class StringParser : ChannelParserBase
    {
        private AsyncAutoResetEvent dataReadEvent;
        private Queue<string> queue;
        private const uint bufferSize = 512;
        private static char[] lineBreak = { '\r', '\n', '\0' };

        public StringParser(SocketObject socket) : base(socket)
        {
            queue = new Queue<string>();
            dataReadEvent = new AsyncAutoResetEvent();
        }

        public override async void ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                socket.ConnectionStatus = ConnectionStatus.Connected;
                using (DataReader dataReader = new DataReader(args.Socket.InputStream))
                {
                    CancellationToken cancellationToken = socket.CancellationTokenSource.Token;
                    //setup
                    lock (socket.CancellationTokenSource)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        dataReader.InputStreamOptions = InputStreamOptions.Partial;
                    }

                    uint bytesRead = await dataReader.LoadAsync(bufferSize).AsTask(cancellationToken).ConfigureAwait(false);
                    while (bytesRead > 0)
                    {
                        queue.Enqueue(dataReader.ReadString(bytesRead));
                        dataReadEvent.Set();
                        bytesRead = await dataReader.LoadAsync(bufferSize).AsTask(cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception exception)
            {
                socket.ConnectionStatus = ConnectionStatus.Failed;
                Debug.WriteLine(string.Format("Error receiving data: {0}", exception.Message));
            }
        }
        public override async void ParseData()
        {
            StringBuilder builder = new StringBuilder();
            while (true)
            {
                await dataReadEvent.WaitAsync();
                while (queue.Count > 0)
                {
                    string buffer = queue.Dequeue();
                    string[] lines = buffer.Split(lineBreak);
                    if (buffer.Length > 1)
                    {
                        foreach (string line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                builder.Append(line);
                                Debug.WriteLine(builder.ToString());
                                builder.Clear();
                            }
                        }
                    }
                    else
                        builder.Append(buffer);
                }
            }
        }
    }
}