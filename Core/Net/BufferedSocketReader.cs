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

        public Socket Socket { get; private set; }
        public MemoryBuffer Buffer { get; private set; }
        public long LastMessageTime { get; private set; }

        public BufferedSocketReader(Socket socket)
        {
            Socket = socket;
            Buffer = new MemoryBuffer();
            LastMessageTime = Time.CurrentTimeMs;
        }

        public int Read()
        {
            byte[] data = new byte[MAX_BUFFER];
            int len = Socket.Receive(data);
            if(0 == len) {
                return 0;
            }

            Buffer.Write(data, 0, len);
            LastMessageTime = Time.CurrentTimeMs;

            return len;
        }

        /*public int ReadFrom(ref EndPoint endpoint)
        {
            byte[] data = new byte[MAX_BUFFER];
            int len = Socket.ReceiveFrom(data, ref endpoint);
            if(0 == len) {
                return 0;
            }

            Buffer.Write(data, 0, len);
            LastMessageTime = Time.CurrentTimeMs;

            return len;
        }*/

        public void Read(byte[] data, int offset, int len)
        {
            Buffer.Write(data, offset, len);
            LastMessageTime = Time.CurrentTimeMs;
        }
    }
}
