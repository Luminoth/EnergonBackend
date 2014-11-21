﻿using System.Net;
using System.Net.Sockets;

using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class SocketState
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SocketState));

#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        private readonly object _lock = new object();

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

        public int PollAndRead()
        {
            lock(_lock) {
                int count = _reader.PollAndRead();
                if(count > 0) {
                    LastMessageTime = Time.CurrentTimeMs;
                }
                return count;
            }
        }

        public void BufferData(byte[] data)
        {
            lock(_lock) {
                _reader.BufferData(data);
            }
        }

        public void BufferData(byte[] data, int offset, int count)
        {
            lock(_lock) {
                _reader.BufferData(data, offset, count);
            }
        }

        public int Send(byte[] buffer)
        {
            lock(_lock) {
                Logger.Debug("Socket state " + Id + " sending " + buffer.Length + " bytes");
                return Socket.Send(buffer);
            }
        }

        public void ShutdownAndClose(bool reuseSocket)
        {
            lock(_lock) {
                if(!HasSocket) {
                    return;
                }

                Socket.Shutdown(SocketShutdown.Both);
                Socket.Disconnect(reuseSocket);
                Socket.Close();
            }
        }
    }
}
