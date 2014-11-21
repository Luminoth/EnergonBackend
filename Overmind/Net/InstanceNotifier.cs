using System;
using System.Configuration;
using System.Net;

using EnergonSoftware.Core.Configuration;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Overmind.MessageHandlers;

using log4net;

namespace EnergonSoftware.Overmind.Net
{
    internal sealed class InstanceNotifier
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InstanceNotifier));

#region Singleton
        private static InstanceNotifier _instance = new InstanceNotifier();
        public static InstanceNotifier Instance { get { return _instance; } }
#endregion

        private UdpListener _listener = new UdpListener(new InstanceNotifierSessionFactory());
        private SessionManager _sessions = new SessionManager();

        public void Start(ListenAddressConfigurationElementCollection listenAddresses)
        {
            Logger.Debug("Opening multicast listener sockets...");
            _listener.CreateMulticastSockets(listenAddresses);

            Logger.Debug("Starting session manager...");
            _sessions.SessionTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["sessionTimeout"]);
            _sessions.Start(new MessageHandlerFactory());
        }

        public void Stop()
        {
            Logger.Debug("Closing multicast sockets...");
            _listener.CloseSockets();

            Logger.Debug("Stopping session manager...");
            _sessions.Stop();
        }

        public void Run()
        {
            _listener.Poll(_sessions);

            _sessions.PollAndRun();
            _sessions.Cleanup();
        }

        public void Login(string username, EndPoint endpoint)
        {
        }

        public void Logout(string username)
        {
        }

        private InstanceNotifier()
        {
        }
    }
}