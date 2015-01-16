using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Net
{
    public static class NetUtil
    {
        public static async Task<Socket> ConnectAsync(string host, int port, SocketType socketType, ProtocolType protocolType)
        {
            bool useIPv6 = Convert.ToBoolean(ConfigurationManager.AppSettings["useIPv6"]);

            IPHostEntry hostEntry = await Dns.GetHostEntryAsync(host).ConfigureAwait(false);
            foreach(IPAddress address in hostEntry.AddressList) {
                if(AddressFamily.InterNetwork != address.AddressFamily && (AddressFamily.InterNetworkV6 != address.AddressFamily || !useIPv6)) {
                    continue;
                }

                EndPoint endPoint = new IPEndPoint(address, port);
                Socket socket = new Socket(endPoint.AddressFamily, socketType, protocolType);
                try {
                    await socket.ConnectAsync(endPoint).ConfigureAwait(false);
                } catch(SocketException) {
                    continue;
                }
                return socket;
            }

            return null;
        }

        public static Socket CreateMulticastListener(IPAddress iface, int port, IPAddress group, int ttl)
        {
            EndPoint endpoint = new IPEndPoint(iface, port);

            Socket listener = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Bind(endpoint);

// TODO: support ipv6 here
            listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(group, iface));
            listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);

            return listener;
        }

        public static async Task<Socket> ConnectMulticastAsync(IPAddress group, int port, int ttl)
        {
            EndPoint endPoint = new IPEndPoint(group, port);
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

// TODO: support ipv6 here
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(group));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
            await socket.ConnectAsync(endPoint).ConfigureAwait(false);

            return socket;
        }

        public static bool CompareEndPoints(string a, EndPoint b)
        {
            string[] aparts = a.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if(aparts.Length < 1) {
                return false;
            }

            string[] bparts = b.ToString().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if(bparts.Length < 1) {
                return false;
            }

            return aparts[0].Equals(bparts[0], StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
