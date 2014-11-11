using System.Net;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Net
{
    public sealed class SocketState
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SocketState));

#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        public readonly int Id;

        private Socket _socket;
        public Socket Socket
        {
            private get { return _socket; }
            set {
                _socket = value;
                _reader = new BufferedSocketReader(_socket);
            }
        }

        public bool HasSocket { get { return null != _socket; } }
        public bool Connecting { get; set; }
        public bool Connected { get { return HasSocket && _socket.Connected; } }
        public EndPoint RemoteEndPoint { get { return HasSocket ? _socket.RemoteEndPoint : null; } }

        private BufferedSocketReader _reader;
        public MemoryBuffer Buffer { get { return HasSocket ? _reader.Buffer : null; } }

        public long LastMessageTime { get; private set; }

        public SocketState()
        {
            Id = NextId;
        }

        public SocketState(Socket socket)
        {
            Id = NextId;
            Socket = socket;
        }

        public int Poll()
        {
            int len = _reader.Poll();
            if(len > 0) {
                _logger.Debug("Socket state " + Id + " read " + len + " bytes");
                LastMessageTime = Time.CurrentTimeMs;
            }
            return len;
        }

        public int Send(byte[] buffer)
        {
            _logger.Debug("Socket state " + Id + " sending " + buffer.Length + " bytes");
            return Socket.Send(buffer);
        }

        public void ShutdownAndClose(bool reuseSocket)
        {
            if(!HasSocket) {
                return;
            }

            Socket.Shutdown(SocketShutdown.Both);
            Socket.Disconnect(reuseSocket);
            Socket.Close();
        }
    }
}
