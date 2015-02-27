using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.IO;
using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Net.Sockets
{
    public sealed class SocketState : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SocketState));

#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        public int Id { get; private set; }

        private SemaphoreSlim _socketLock = new SemaphoreSlim(1);
        private Socket _socket;
        public Socket Socket
        {
            private get { return _socket; }

            set
            {
                _socketLock.Wait();
                try {
                    _socket = value;
                } finally {
                    _socketLock.Release();
                }
            }
        }

        public bool Connecting { get; set; }
        public bool Connected { get { return null != Socket && Socket.Connected; } }
        public EndPoint RemoteEndPoint { get { return null != Socket ? Socket.RemoteEndPoint : null; } }

        public LockingMemoryStream Buffer { get; private set; }

        public long LastMessageTimeMS { get; private set; }

        public SocketState()
        {
            Id = NextId;
            Buffer = new LockingMemoryStream();
        }

        public SocketState(Socket socket) : this()
        {
            _socket = socket;
        }

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(null != Socket) {
                    Socket.Dispose();
                }

                _socketLock.Dispose();
                Buffer.Dispose();
            }
        }
#endregion

        public async Task<int> PollAndReceiveAllAsync()
        {
            await _socketLock.WaitAsync().ConfigureAwait(false);
            await Buffer.LockAsync().ConfigureAwait(false);

            try {
                if(null == Socket) {
                    return -1;
                }

                int count = await Socket.PollAndReceiveAllAsync(Buffer).ConfigureAwait(false);
                if(count > 0) {
                    LastMessageTimeMS = Time.CurrentTimeMs;
                }

                return count;
            } finally {
                Buffer.Release();
                _socketLock.Release();
            }
        }

        public async Task<int> SendAsync(byte[] buffer)
        {
            await _socketLock.WaitAsync().ConfigureAwait(false);
            try {
                if(null == Socket) {
                    return -1;
                }

                Logger.Debug("Socket state " + Id + " sending " + buffer.Length + " bytes");
                return await Socket.SendAsync(buffer).ConfigureAwait(false);
            } finally {
                _socketLock.Release();
            }
        }

        public async Task ShutdownDisconnectCloseAsync(bool reuseSocket)
        {
            await _socketLock.WaitAsync().ConfigureAwait(false);
            try {
                if(null == Socket) {
                    return;
                }

                await Socket.ShutdownDisconnectCloseAsync(SocketShutdown.Both, reuseSocket);
                _socket = null;
            } finally {
                _socketLock.Release();
            }
        }
    }
}
