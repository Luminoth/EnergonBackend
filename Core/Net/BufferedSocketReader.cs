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
        private static int MAX_BUFFER = 1024;

        private readonly object _lock = new object();

        private readonly Socket _socket;
        public readonly MemoryBuffer Buffer;

        public BufferedSocketReader(Socket socket)
        {
            _socket = socket;
            Buffer = new MemoryBuffer();
        } 

        public bool PollAndRead(out int count)
        {
            lock(_lock) {
                count = 0;
                while(_socket.Connected && _socket.Poll(100, SelectMode.SelectRead)) {
                    int len = Read();
                    if(len <= 0) {
                        return false;
                    }
                    count += len;
                }
                return true;
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
