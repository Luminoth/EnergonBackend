using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class SocketState : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SocketState));

#region Id Generator
        private static int _nextId = 0;
        private static int NextId { get { return ++_nextId; } }
#endregion

        public readonly int Id;

        public Socket Socket { private get; set; }
        public bool Connecting { get; set; }
        public bool Connected { get { return null != Socket && Socket.Connected; } }
        public EndPoint RemoteEndPoint { get { return null != Socket ? Socket.RemoteEndPoint : null; } }

        public readonly MemoryStream Buffer = new MemoryStream();

        public long LastMessageTimeMS { get; private set; }

        public SocketState()
        {
            Id = NextId;
        }

        public SocketState(Socket socket) : this()
        {
            Socket = socket;
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
                if(null != Socket) {
                    Socket.Dispose();
                }
                Buffer.Dispose();
            }
        }
#endregion

        public async Task<int> PollAndReceiveAllAsync()
        {
            if(null == Socket) {
                return -1;
            }

            int count = await Socket.PollAndReceiveAllAsync(Buffer).ConfigureAwait(false);
            if(count > 0) {
                LastMessageTimeMS = Time.CurrentTimeMs;
            }
            return count;
        }

        public async Task<int> SendAsync(byte[] buffer)
        {
            if(null == Socket) {
                return -1;
            }

            Logger.Debug("Socket state " + Id + " sending " + buffer.Length + " bytes");
            return await Socket.SendAsync(buffer).ConfigureAwait(false);
        }

        public async Task ShutdownDisconnectCloseAsync(bool reuseSocket)
        {
            if(null == Socket) {
                return;
            }

            await Socket.ShutdownDisconnectCloseAsync(SocketShutdown.Both, reuseSocket);
            Socket = null;
        }
    }
}
