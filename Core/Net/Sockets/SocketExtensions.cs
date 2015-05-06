using System.IO;
using System.Threading.Tasks;

namespace System.Net.Sockets
{
    public static class SocketExtensions
    {
#region Dgram Helpers
        public static Socket AcceptFrom(this Socket socket)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteEndpoint = (EndPoint)sender;

            // peek at the available data to get at the remote endpoint
            byte[] buffer = new byte[1];
            int len = socket.ReceiveFrom(buffer, SocketFlags.Peek, ref remoteEndpoint);
            if(len < 1) {
                return null;
            }

            Socket remote = null;
            try {
                remote = new Socket(socket.AddressFamily, socket.SocketType, socket.ProtocolType);
                remote.Connect(remoteEndpoint);
                return remote;
            } catch(Exception) {
                if(null != remote) {
                    remote.Dispose();
                }

                throw;
            }
        }

        public static async Task<Socket> AcceptFromAsync(this Socket socket)
        {
            try {
                return await Task.Run(() => socket.AcceptFrom()).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }
#endregion

#region Async Helpers
        public static async Task<Socket> AcceptAsync(this Socket socket)
        {
            try {
                return await Task.Run(() => socket.Accept()).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        public static async Task ConnectAsync(this Socket socket, EndPoint endPoint)
        {
            try {
                await Task.Run(() => socket.Connect(endPoint)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        public static async Task ConnectAsync(this Socket socket, IPAddress address, int port)
        {
            try {
                await Task.Run(() => socket.Connect(address, port)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        public static async Task ConnectAsync(this Socket socket, IPAddress[] addresses, int port)
        {
            try {
                await Task.Run(() => socket.Connect(addresses, port)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        public static async Task ConnectAsync(this Socket socket, string host, int port)
        {
            try {
                await Task.Run(() => socket.Connect(host, port)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        public static async Task DisconnectAsync(this Socket socket, bool reuseSocket)
        {
            try {
                await Task.Run(() => socket.Disconnect(reuseSocket)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        public static async Task<int> ReceiveAsync(this Socket socket, byte[] buffer)
        {
            try {
                return await Task.Run(() => socket.Receive(buffer)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        /*public static async Task<int> ReceiveFromAsync(this Socket socket, byte[] buffer, ref EndPoint endPoint)
        {
            try {
                return await Task.Run(() => socket.ReceiveFrom(buffer, ref endPoint)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }*/

        //// TODO: add more receive methods

        public static async Task<int> SendAsync(this Socket socket, byte[] buffer)
        {
            try {
                return await Task.Run(() => socket.Send(buffer)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        public static async Task<int> SendToAsync(this Socket socket, byte[] buffer, EndPoint endPoint)
        {
            try {
                return await Task.Run(() => socket.SendTo(buffer, endPoint)).ConfigureAwait(false);
            } catch(AggregateException e) {
                throw e.InnerException;
            }
        }

        //// TODO: add more send methods
#endregion

#region Misc Helpers
        public static async Task ShutdownDisconnectCloseAsync(this Socket socket, SocketShutdown how, bool reuseSocket)
        {
            socket.Shutdown(how);
            if(socket.Connected) {
                await socket.DisconnectAsync(reuseSocket);
            }

            socket.Close();
        }
#endregion

#region Stream Helpers
        public static async Task<int> ReceiveAsync(this Socket socket, Stream stream)
        {
            byte[] data = new byte[socket.Available];
            int len = await socket.ReceiveAsync(data).ConfigureAwait(false);
            if(len <= 0) {
                return len;
            }

            await stream.WriteAsync(data, 0, len).ConfigureAwait(false);
            return len;
        }

        public static async Task<int> PollAndReceiveAllAsync(this Socket socket, int microSeconds, Stream stream)
        {
            int total = 0;
            while(socket.Poll(microSeconds, SelectMode.SelectRead)) {
                int len = await socket.ReceiveAsync(stream).ConfigureAwait(false);
                if(len <= 0) {
                    return total > 0 ? total : -1;
                }

                total += len;
            }

            return total;
        }
#endregion
    }
}
