using System;
using System.Net;
using System.Net.Sockets;

namespace EnergonSoftware.Core.Net
{
    public sealed class AsyncConnectEventArgs
    {
#region Events
        public delegate void OnConnectSuccessHandler(Socket socket);
        public event OnConnectSuccessHandler OnConnectSuccess;
        
        public delegate void OnConnectFailedHandler(SocketError error);
        public event OnConnectFailedHandler OnConnectFailed;
#endregion

        public AsyncConnectEventArgs()
        {
        }

        public void ConnectSuccess(Socket socket)
        {
            if(null != OnConnectSuccess) {
                OnConnectSuccess(socket);
            }
        }

        public void ConnectFailed(SocketError error)
        {
            if(null != OnConnectFailed) {
                OnConnectFailed(error);
            }
        }
    }

    public static class NetUtil
    {
        private sealed class AsyncConnectContext
        {
            public AsyncConnectEventArgs EventHandler;
            public IPAddress[] AddressList;
            public int CurrentAddressIdx;
            public int Port;
            public Socket Socket;

            public IPAddress CurrentAddress
            {
                get
                {
                    if(null == AddressList || CurrentAddressIdx < 0 || CurrentAddressIdx >= AddressList.Length) {
                        return null;
                    }
                    return AddressList[CurrentAddressIdx];
                }
            }
        }

        public static Socket Connect(string host, int port)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);
            foreach(IPAddress address in hostEntry.AddressList) {
                EndPoint endPoint = new IPEndPoint(address, port);
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try {
                    socket.Connect(endPoint);
                } catch(Exception) {
                    continue;
                }
                return socket;
            }

            return null;
        }

        private static void ConnectAsyncCallback(object sender, SocketAsyncEventArgs ea)
        {
            AsyncConnectContext context = (AsyncConnectContext)ea.UserToken;

            if(SocketError.Success == ea.SocketError) {
                context.EventHandler.ConnectSuccess(context.Socket);
                return;
            }

            context.CurrentAddressIdx++;
            if(!DoConnectAsync(context)) {
                context.EventHandler.ConnectFailed(ea.SocketError);
            }
        }

        private static bool DoConnectAsync(AsyncConnectContext context)
        {
            IPAddress address = context.CurrentAddress;
            if(null == address) {
                return false;
            }

            EndPoint endPoint = new IPEndPoint(address, context.Port);
            context.Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs ea = new SocketAsyncEventArgs();
            ea.RemoteEndPoint = endPoint;
            ea.UserToken = context;
            ea.Completed += ConnectAsyncCallback;
            context.Socket.ConnectAsync(ea);

            return true;
        }

        public static bool ConnectAsync(string host, int port, AsyncConnectEventArgs args)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(host);

            AsyncConnectContext context = new AsyncConnectContext();
            context.EventHandler = args;
            context.AddressList = hostEntry.AddressList;
            context.CurrentAddressIdx = 0;
            context.Port = port;

            if(!DoConnectAsync(context)) {
                context.EventHandler.ConnectFailed(SocketError.HostNotFound);
                return false;
            }
            return true;
        }
    }
}
