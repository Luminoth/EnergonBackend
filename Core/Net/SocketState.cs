using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class SocketState : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SocketState));

#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        public readonly int Id;

        private Socket _socket;
        public Socket Socket
        {
            set {
                _socket = value;
                _reader = null == _socket ? null : new BufferedSocketReader(_socket);
            }
        }

        public bool IsValid { get { return null != _socket && null != _reader; } }

        public bool Connecting { get; set; }
        public bool Connected { get { return null != _socket && _socket.Connected; } }
        public EndPoint RemoteEndPoint { get { return null != _socket ? _socket.RemoteEndPoint : null; } }

        private BufferedSocketReader _reader;
        public MemoryBuffer Buffer { get { return null != _reader ? _reader.Buffer : null; } }

        public long LastMessageTime { get; private set; }

        public SocketState()
        {
            Id = NextId;
        }

        public SocketState(Socket socket) : this()
        {
            Socket = socket;
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
                if(null != _socket) {
                    _socket.Dispose();
                }
            }
        }
#endregion

        public async Task<int> PollAndReadAsync()
        {
            if(!IsValid) {
                return -1;
            }

            int count = await _reader.PollAndReadAsync().ConfigureAwait(false);
            if(count > 0) {
                LastMessageTime = Time.CurrentTimeMs;
            }
            return count;
        }

        public async Task<int> SendAsync(byte[] buffer)
        {
            if(!IsValid) {
                return -1;
            }

            Logger.Debug("Socket state " + Id + " sending " + buffer.Length + " bytes");
            return await Task.Run(() => _socket.Send(buffer)).ConfigureAwait(false);
        }

        public async Task ShutdownAndCloseAsync(bool reuseSocket)
        {
            if(!IsValid) {
                return;
            }

            _socket.Shutdown(SocketShutdown.Both);
            if(_socket.Connected) {
                await Task.Run(() => _socket.Disconnect(reuseSocket)).ConfigureAwait(false);
            }
            _socket.Close();

            _reader = null;
            _socket = null;
        }
    }
}
