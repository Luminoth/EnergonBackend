using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;

namespace EnergonSoftware.Core.Net
{
    public sealed class SessionManager
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SessionManager));

        private readonly object _lock = new object();
 
        private List<Session> _sessions = new List<Session>();
        private MessageProcessor _processor = new MessageProcessor();

        public long SessionTimeout { get; set; }

        public SessionManager()
        {
            SessionTimeout = -1;
        }

        public void Start(IMessageHandlerFactory factory)
        {
            lock(_lock) {
                _processor.Start(factory);
            }
        }

        public void Stop()
        {
            lock(_lock) {
                _logger.Info("Closing all sessions...");
                DisconnectAll();
                _sessions.Clear();

                _processor.Stop();
            }
        }

        public void Add(Session session)
        {
            lock(_lock) {
                session.Timeout = SessionTimeout;
                _sessions.Add(session);

                _logger.Info("Added new session: " + session.Id);
            }
        }

        public bool Contains(EndPoint remoteEndpoint)
        {
            lock(_lock) {
                return _sessions.Exists(session => session.Connected && remoteEndpoint.Equals(session.RemoteEndPoint));
            }
        }

        public Session Get(EndPoint remoteEndpoint)
        {
            lock(_lock) {
                return _sessions.Find(session => session.Connected && remoteEndpoint.Equals(session.RemoteEndPoint));
            }
        }

        public void DisconnectAll()
        {
            lock(_lock) {
                _sessions.ForEach(session => session.Disconnect());
            }
        }

        public void Cleanup()
        {
            int count = 0;
            lock(_lock) {
                count = _sessions.RemoveAll(session =>
                    {
                        if(!session.Connecting && !session.Connected) {
                            _processor.RemoveSession(session.Id);
                            return true;
                        }
                        return false;
                    }
                );
            }

            if(count > 0) {
                _logger.Info("Removed " + count + " disconnected sessions");
            }
        }

        public void PollAndRun()
        {
            lock(_lock) {
                _sessions.ForEach(session =>
                    {
                        int count = session.PollAndRead();
                        if(count < 0) {
                            session.Disconnect("Socket closed!");
                            return;
                        }

                        if(session.TimedOut) {
                            _logger.Info("Session " + session.Id + " timed out!");
                            session.Disconnect("Timed Out!");
                            return;
                        }

                        if(session.Connected) {
                            session.Run(_processor);
                        }
                    }
                );
            }
        }

        public void SendMessage(IMessage message)
        {
            lock(_lock) {
                _sessions.ForEach(session => session.SendMessage(message));
            }
        }
    }
}
