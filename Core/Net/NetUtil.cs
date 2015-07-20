using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EnergonSoftware.Core.Net
{
    /// <summary>
    /// Useful network utility methods.
    /// </summary>
    public static class NetUtil
    {
#region Socket Connectors
        /// <summary>
        /// Creates a Socket and connects it to the given host or address.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="socketType">The socket type to use when creating the socket.</param>
        /// <param name="protocolType">The protocol to use when creating the socket.</param>
        /// <param name="useIPv6">If set to <c>true</c>, will connect to IPv6 addresses, otherwise only IPv4 addresses will be used.</param>
        /// <returns>The newly connected socket or null if no addresses could be resolved.</returns>
        /// <remarks>
        /// This method handles DNS resolution and iterates over the resolved addresses until one connects.
        /// Remaining addresses are skipped.
        /// </remarks>
        public static async Task<Socket> ConnectAsync(string hostNameOrAddress, int port, SocketType socketType, ProtocolType protocolType, bool useIPv6)
        {
            IPHostEntry hostEntry = await Dns.GetHostEntryAsync(hostNameOrAddress).ConfigureAwait(false);
            foreach(IPAddress address in hostEntry.AddressList) {
                // skip non-IPv4 and IPv6 (when not using IPv6) addresses
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

        /// <summary>
        /// Creates a Socket and binds it to the given multicast interface.
        /// </summary>
        /// <param name="iface">The interface address to bind to.</param>
        /// <param name="port">The port to bind to.</param>
        /// <param name="group">The multicast group to join.</param>
        /// <param name="ttl">The TTL of the socket.</param>
        /// <returns>The newly created multicast listener socket, bound to the given interface and joined to the given multicast group.</returns>
        /// <remarks>
        /// Does not support IPv6.
        /// </remarks>
        public static Socket CreateMulticastListener(IPAddress iface, int port, IPAddress group, int ttl)
        {
            EndPoint endpoint = new IPEndPoint(iface, port);

            Socket listener = null;
            try {
                listener = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listener.Bind(endpoint);

                // TODO: support ipv6 here
                listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(group, iface));
                listener.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);

                return listener;
            } catch(Exception) {
                if(null != listener) {
                    listener.Dispose();
                }

                throw;
            }
        }

        /// <summary>
        /// Creates a Socket and connects it to the given multicast group.
        /// </summary>
        /// <param name="group">The multicast group to connect to.</param>
        /// <param name="port">The port to connect to.</param>
        /// <param name="ttl">The TTL of the socket.</param>
        /// <returns></returns>
        /// <remarks>
        /// Does not support IPv6.
        /// </remarks>
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
#endregion

        /// <summary>
        /// Compares a string-value EndPoint with an EndPoint object.
        /// Does not consider the EndPoint port, only the address.
        /// </summary>
        /// <param name="a">A string-value EndPoint (host:port).</param>
        /// <param name="b">The EndPoint to compare against.</param>
        /// <returns>True if the EndPoints represent the same EndPoint.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// a
        /// or
        /// b
        /// </exception>
        public static bool CompareEndPoints(string a, EndPoint b)
        {
            if(null == a) {
                throw new ArgumentNullException("a");
            }

            if(null == b) {
                throw new ArgumentNullException("b");
            }

            string[] aparts = a.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if(aparts.Length < 1) {
                return false;
            }

            string[] bparts = b.ToString().Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if(bparts.Length < 1) {
                return false;
            }

            return aparts[0].Equals(bparts[0], StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
