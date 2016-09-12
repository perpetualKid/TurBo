using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Communication.Channels
{
    public abstract class SocketObject
    {
        protected object cancelLock = new Object();
        protected CancellationTokenSource cancellationTokenSource;
        protected ConnectionStatus connectionStatus;
        protected uint bytesRead;
        protected uint bytesWritten;

        public event EventHandler<ConnectionStatusChangedEventArgs> OnConnectionStatusChanged;

        public void CancelSocketTask()
        {
            lock (cancelLock)
            {
                if ((cancellationTokenSource != null) && (!cancellationTokenSource.IsCancellationRequested))
                {
                    cancellationTokenSource.Cancel();
                    // Existing IO already has a local copy of the old cancellation token so this reset won't affect it 
                    ResetCancellationTokenSource();
                }
            }
        }

        public void ResetCancellationTokenSource()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Dispose();
            }
            // Create a new cancellation token source so that can cancel all the tokens again 
            cancellationTokenSource = new CancellationTokenSource();
            // Hook the cancellation callback (called whenever Task.cancel is called) 
            //cancellationTokenSource.Token.Register(() => NotifyCancelingTask()); 
        }

        public CancellationTokenSource CancellationTokenSource { get { return this.cancellationTokenSource; } }

        public ConnectionStatus ConnectionStatus
        {
            get { return this.connectionStatus; }
            internal set
            {
                if (value != connectionStatus)
                {
                    this.connectionStatus = value;
                    OnConnectionStatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs { Status = value });
                }
            }
        }

    }
}
