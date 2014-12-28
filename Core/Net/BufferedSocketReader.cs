using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Core.Net
{
    public sealed class BufferedSocketReader : IDisposable
    {
        private readonly Socket _socket;
        public readonly MemoryBuffer Buffer = new MemoryBuffer();

        private BufferedSocketReader()
        {
        }

        public BufferedSocketReader(Socket socket) : this()
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
                Buffer.Dispose();
            }
        }
#endregion

        // reads from the socket as long
        // as there is data to be read
        // returns the total number of bytes read
        // or -1 on socket closed
        public int PollAndRead()
        {
            int count = 0;
            while(_socket.Poll(100, SelectMode.SelectRead)) {
                int len = Read();
                if(len <= 0) {
                    return count > 0 ? count : -1;
                }
                count += len;
            }
            return count;
        }

        private int Read()
        {
            byte[] data = new byte[_socket.Available];
            int len = _socket.Receive(data);
            if(len <= 0) {
                return len;
            }

            Buffer.Write(data, 0, len);
            return len;
        }
    }
}
