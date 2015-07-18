using System.IO;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Net.Sockets
{
    public static class SocketExtensions
    {
#region Dgram Helpers
        public static Socket AcceptFrom(this Socket socket)
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteEndpoint = sender;

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

        public static Task<Socket> AcceptFromAsync(this Socket socket)
        {
            return Task.Run(() => socket.AcceptFrom());
        }
#endregion

#region Async Helpers
        public static Task<Socket> AcceptAsync(this Socket socket)
        {
            return Task.Run(() => socket.Accept());
        }

        public static Task ConnectAsync(this Socket socket, EndPoint endPoint)
        {
            return Task.Run(() => socket.Connect(endPoint));
        }

        public static Task ConnectAsync(this Socket socket, IPAddress address, int port)
        {
            return Task.Run(() => socket.Connect(address, port));
        }

        public static Task ConnectAsync(this Socket socket, IPAddress[] addresses, int port)
        {
            return Task.Run(() => socket.Connect(addresses, port));
        }

        public static Task ConnectAsync(this Socket socket, string host, int port)
        {
            return Task.Run(() => socket.Connect(host, port));
        }

        public static Task DisconnectAsync(this Socket socket, bool reuseSocket)
        {
            return Task.Run(() => socket.Disconnect(reuseSocket));
        }

        public static Task<int> ReceiveAsync(this Socket socket, byte[] buffer)
        {
            return Task.Run(() => socket.Receive(buffer));
        }

        /*public static Task<int> ReceiveFromAsync(this Socket socket, byte[] buffer, ref EndPoint endPoint)
        {
            return Task.Run(() => socket.ReceiveFrom(buffer, ref endPoint));
        }*/

// TODO: add more receive methods

        public static Task<int> SendAsync(this Socket socket, byte[] buffer)
        {
            return Task.Run(() => socket.Send(buffer));
        }

        public static Task<int> SendToAsync(this Socket socket, byte[] buffer, EndPoint endPoint)
        {
            return Task.Run(() => socket.SendTo(buffer, endPoint));
        }

// TODO: add more send methods
#endregion

#region Misc Helpers
        public static async Task ShutdownDisconnectCloseAsync(this Socket socket, SocketShutdown how, bool reuseSocket)
        {
            socket.Shutdown(how);

            if(socket.Connected) {
                await socket.DisconnectAsync(reuseSocket).ConfigureAwait(false);
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
                if(socket.Available <= 0) {
                    return total > 0 ? total : -1;
                }

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
