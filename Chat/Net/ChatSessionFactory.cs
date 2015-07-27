using System;
using System.Configuration;
using System.Net.Sockets;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Chat.Net
{
    internal sealed class ChatSessionFactory : INetworkSessionFactory
    {
        public NetworkSession Create(Socket socket)
        {
            ChatSession session = null;
            try {
                session = new ChatSession(socket)
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
