using System;
using System.Collections.Generic;
using System.Net.Sockets;

using log4net;

namespace EnergonSoftware.Authenticator.Net
{
    sealed class SessionManager
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SessionManager));

#region Singleton
        private static SessionManager _instance = new SessionManager();
        public static SessionManager Instance { get { return _instance; } }
#endregion

        private List<Session> _sessions = new List<Session>();

        public void AddSession(Socket socket)
        {
            Session session = new Session(socket);
            _sessions.Add(session);
            _logger.Info("Added new session " + session.Id);
        }

        public void Cleanup()
        {
            int count = _sessions.RemoveAll(s => !s.Connected);
            if(count > 0) {
                _logger.Info("Removed " + count + " disconnected sessions");
            }
        }

        private void PollAndRun(Session session)
        {
            session.Poll();

            if(session.TimedOut) {
                _logger.Info("Session " + session.Id + " timed out!");
                session.Disconnect();
                return;
            }

            if(session.Connected) {
                session.Run();
            }
        }

        public void PollAndRun()
        {
            _sessions.ForEach(s => PollAndRun(s));
        }

        public void CloseAll()
        {
            _logger.Info("Closing all sessions...");
            _sessions.ForEach(s => s.Disconnect());
            _sessions.Clear();
        }

        private SessionManager()
        {
        }
    }
}
