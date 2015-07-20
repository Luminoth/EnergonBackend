using System.Net.Sockets;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// Factory for creating network sessions.
    /// </summary>
    public interface INetworkSessionFactory
    {
        /// <summary>
        /// Creates a new network session that wraps the specified socket.
        /// </summary>
        /// <param name="socket">The socket to wrap.</param>
        /// <returns>A new NetworkSession.</returns>
        NetworkSession Create(Socket socket);
    }
}
