using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Messages;
using EnergonSoftware.Core.Messages.Formatter;
using EnergonSoftware.Core.Messages.Parser;

using log4net;

namespace EnergonSoftware.Core.Net
{
    public sealed class SessionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SessionManager));

        private readonly object _lock = new object();
 
        private readonly List<Session> _sessions = new List<Session>();

        public void Add(Session session)
        {
            lock(_lock) {
                _sessions.Add(session);
            }
            Logger.Info("Added new session: " + session.Id);
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
            Logger.Info("Disconnecting all sessions...");
            lock(_lock) {
                _sessions.ForEach(session => session.Disconnect());
                _sessions.Clear();
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
                            session.Run();
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
