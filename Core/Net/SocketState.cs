using System.Net.Sockets;

namespace EnergonSoftware.Core.Net
{
    public sealed class SocketState
    {
#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        public int Id { get; private set; }

#region Socket Properties
        // NOTE: these are not maintained by the socket state
        // they're just caller storage when needed
        public string Host { get; set; }
        public int Port { get; set; }
        public bool Connecting { get; set; }

        private Socket _socket;
        public Socket Socket
        {
            get { return _socket; }
            set {
                _socket = value;
                Reader = null == _socket ? null : new BufferedSocketReader(_socket);
            }
        }
        public bool HasSocket { get { return null != _socket; } }

        public bool Connected { get { return HasSocket && Socket.Connected; } }
#endregion

        public BufferedSocketReader Reader { get; private set; }

        public SocketState()
        {
            Id = NextId;
        }

        public SocketState(Socket socket)
        {
            Id = NextId;
            Socket = socket;
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

        public void Reset()
        {
            Host = null;
            Port = 0;
            Connecting = false;

            Socket = null;
        }
    }
}
