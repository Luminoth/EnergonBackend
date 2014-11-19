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
        private readonly object _lock = new object();

        private readonly Socket _socket;
        public readonly MemoryBuffer Buffer;

        public BufferedSocketReader(Socket socket)
        {
            _socket = socket;
            Buffer = new MemoryBuffer();
        } 

        // reads from the socket as long
        // as there is data to be read
        // returns the total number of bytes read
        // or -1 on socket closed
        public int PollAndRead()
        {
            lock(_lock) {
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
