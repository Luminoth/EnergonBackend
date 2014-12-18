using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Parser;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class SessionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SessionManager));

        private readonly object _lock = new object();
 
        private readonly List<Session> _sessions = new List<Session>();
        private readonly IMessagePacketParser _parser = new NetworkPacketParser();

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
                Logger.Info("Closing all sessions...");
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

                Logger.Info("Added new session: " + session.Id);
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
                            return true;
                        }
                        return false;
                    }
                );
            }

            if(count > 0) {
                Logger.Info("Removed " + count + " disconnected sessions");
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
                            Logger.Info("Session " + session.Id + " timed out!");
                            session.Disconnect("Timed Out!");
                            return;
                        }

                        if(session.Connected) {
                            session.Run(_parser);
                        }
                    }
                );
            }
        }

        public void BroadcastMessage(IMessage message)
        {
            lock(_lock) {
                _sessions.ForEach(session => session.SendMessage(message));
            }
        }
    }
}
