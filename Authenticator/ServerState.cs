using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Database;

namespace EnergonSoftware.Authenticator
{
    sealed class ServerState
    {
#region Singleton
        private static ServerState _instance = new ServerState();
        public static ServerState Instance { get { return _instance; } }
#endregion

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ServerState));

        public volatile bool Quit = false;

#region Network Properties
        private List<Socket> _listenSockets = new List<Socket>();

        private volatile IMessageFormatter _formatter = new BinaryMessageFormatter();
#endregion

#region Network Methods
        public void CreateSockets()
        {
            ListenAddressesConfigurationSection config = (ListenAddressesConfigurationSection)ConfigurationManager.GetSection("listenAddresses");
            foreach(ListenAddressConfigurationElement listenAddress in config.ListenAddresses) {
                IPEndPoint endpoint = new IPEndPoint(listenAddress.IPAddress, listenAddress.Port);
                _logger.Info("Listening on endpoint " + endpoint + "...");

                Socket socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(endpoint);
                socket.Listen(Int32.Parse(ConfigurationManager.AppSettings["socketBacklog"]));
                _listenSockets.Add(socket);
            }
        }

        public void CloseSockets()
        {
            _logger.Info("Closing listen sockets...");
            _listenSockets.ForEach(s => s.Close());
            _listenSockets.Clear();
        }
#endregion

#region Database Methods
        public DatabaseConnection AcquireDatabaseConnection()
        {
            DatabaseConnection connection = new DatabaseConnection(ConfigurationManager.ConnectionStrings["energonsoftware"]);
            connection.Open();
            return connection;
        }
#endregion

        private ServerState()
        {
        }
    }
}
