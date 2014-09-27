using System.Net.Sockets;

namespace EnergonSoftware.Core.Net
{
    public sealed class SocketState
    {
#region Socket Properties
        private Socket _socket;

        public Socket Socket
        {
            get { return _socket; }
            set {
                _socket = value;
                _reader = null == _socket ? null : new BufferedSocketReader(_socket);
            }
        }
        public bool HasSocket { get { return null != _socket; } }

        public bool Connected { get { return HasSocket && Socket.Connected; } }
#endregion

#region Reader Properties
        private BufferedSocketReader _reader;

        public BufferedSocketReader Reader { get { return _reader; } }
#endregion

        public SocketState()
        {
        }

        public SocketState(Socket socket)
        {
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
            Socket = null;
        }
    }
}
