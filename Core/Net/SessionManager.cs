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
 
        private readonly List<Session> _sessions = new List<Session>();

        public int Count { get { return _sessions.Count; } }

        public void Add(Session session)
        {
            _sessions.Add(session);
            Logger.Info("Added new session: " + session.Id);
        }

        public bool Contains(EndPoint remoteEndpoint)
        {
            return _sessions.Exists(session => session.Connected && remoteEndpoint.Equals(session.RemoteEndPoint));
        }

        public Session Get(EndPoint remoteEndpoint)
        {
            return _sessions.Find(session => session.Connected && remoteEndpoint.Equals(session.RemoteEndPoint));
        }

        public void DisconnectAll()
        {
            Logger.Info("Disconnecting all sessions...");
            Parallel.ForEach<Session>(_sessions, session => session.Disconnect());
            _sessions.Clear();
        }

        public void Cleanup()
        {
            int count = _sessions.RemoveAll(session => !session.Connecting && !session.Connected);
            if(count > 0) {
                Logger.Info("Removed " + count + " disconnected sessions");
            }
        }

        public void PollAndRun()
        {
            Parallel.ForEach<Session>(_sessions, session =>
                {
                    int count = Task.Run(() => session.PollAndRead()).Result;
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
                        Task.Run(() => session.Run()).Wait();
                    }
                }
            );
        }

        public void BroadcastMessage(IMessage message)
        {
            Parallel.ForEach<Session>(_sessions, session => session.SendMessage(message));
        }
    }
}
