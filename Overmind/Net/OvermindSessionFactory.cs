using System;
using System.Configuration;
using System.Net.Sockets;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class OvermindSessionFactory : INetworkSessionFactory
    {
        public NetworkSession Create(Socket socket)
        {
            OvermindSession session = null;
            try {
                session = new OvermindSession(socket)
                {
                    Timeout = TimeSpan.FromMilliseconds(Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeoutMs"])),
                };
                return session;
            } catch(Exception) {
                session?.Dispose();
                throw;
            }
        }
    }
}
