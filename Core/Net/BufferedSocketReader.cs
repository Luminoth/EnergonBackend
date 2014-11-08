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

        private Socket _socket;
        public MemoryBuffer Buffer { get; private set; }

        public BufferedSocketReader(Socket socket)
        {
            _socket = socket;
            Buffer = new MemoryBuffer();
        }

        public int Poll()
        {
            int count = 0;
            while(_socket.Poll(100, SelectMode.SelectRead)) {
                int len = Read();
                if(0 == len) {
                    return -1;
                }
                count += len;
            }
            return count;
        }

        public int Read()
        {
            byte[] data = new byte[MAX_BUFFER];
            int len = _socket.Receive(data);
            if(0 == len) {
                return 0;
            }

            Buffer.Write(data, 0, len);
            return len;
        }
    }
}
