using System.Net.Sockets;

namespace EnergonSoftware.Core.Net.Sessions
{
    public interface ISessionFactory
    {
        Session Create(Socket socket);
    }
}
