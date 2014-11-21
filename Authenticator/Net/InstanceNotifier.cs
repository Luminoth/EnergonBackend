using System;
using System.Configuration;
using System.Net;

using log4net;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Authenticator.MessageHandlers;

namespace EnergonSoftware.Authenticator.Net
{
    sealed class InstanceNotifier
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(InstanceNotifier));

#region Singleton
        private static InstanceNotifier _instance = new InstanceNotifier();
        public static InstanceNotifier Instance { get { return _instance; } }
#endregion

        private UdpListener _listener = new UdpListener(new InstanceNotifierSessionFactory());
        private SessionManager _sessions = new SessionManager();

        public void Start(ListenAddressConfigurationElementCollection listenAddresses)
        {
            _logger.Debug("Opening multicast listener sockets...");
            _listener.CreateMulticastSockets(listenAddresses);

            _logger.Debug("Starting session manager...");
            _sessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            _sessions.Start(new MessageHandlerFactory());
        }

        public void Stop()
        {
            _logger.Debug("Closing multicast sockets...");
            _listener.CloseSockets();

            _logger.Debug("Stopping session manager...");
            _sessions.Stop();
        }

        public void Run()
        {
            _listener.Poll(_sessions);

            _sessions.PollAndRun();
            _sessions.Cleanup();
        }

        public void Authenticating(string username, EndPoint endpoint)
        {
        }

        public void Authenticated(string username, string sessionId, EndPoint endpoint)
        {
        }

        private InstanceNotifier()
        {
        }
    }
}
