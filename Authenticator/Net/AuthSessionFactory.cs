﻿using System;
using System.Configuration;
using System.Net.Sockets;

using EnergonSoftware.Core.Net.Sessions;

namespace EnergonSoftware.Authenticator.Net
{
    internal sealed class AuthSessionFactory : INetworkSessionFactory
    {
        public NetworkSession Create(Socket socket)
        {
            AuthSession session = null;
            try {
                session = new AuthSession(socket)
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
