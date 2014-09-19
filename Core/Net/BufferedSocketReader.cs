using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Net
{
    public class BufferedSocketReader
    {
        private Socket _socket;
        private MemoryBuffer _buffer = new MemoryBuffer();
        public long _lastMessageTime = Time.CurrentTimeMs;

        public Socket Socket { get { return _socket; } }
        public MemoryBuffer Buffer { get { return _buffer; } }
        public long LastMessageTime { get { return _lastMessageTime; } }

        public BufferedSocketReader(Socket socket)
        {
            _socket = socket;
        }

        public int Read()
        {
            byte[] data = new byte[1024];
            int len = _socket.Receive(data);
            if(0 == len) {
                return 0;
            }

            _buffer.Write(data, 0, len);
            _lastMessageTime = Time.CurrentTimeMs;

            return len;
        }

        /*public int ReadFrom(ref EndPoint endpoint)
        {
            byte[] data = new byte[1024];
            int len = _socket.ReceiveFrom(data, ref endpoint);
            if(0 == len) {
                return 0;
            }

            _buffer.Write(data, 0, len);
            _lastMessageTime = Time.CurrentTimeMs;

            return len;
        }*/

        public void Read(byte[] data, int offset, int len)
        {
            _buffer.Write(data, offset, len);
            _lastMessageTime = Time.CurrentTimeMs;
        }
    }
}
