using System.Net.Sockets;

namespace EnergonSoftware.Core.Net.Sessions
{
    public interface INetworkSessionFactory
    {
        NetworkSession Create(Socket socket);
    }
}
