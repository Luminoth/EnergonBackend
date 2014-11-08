using System.Collections.Generic;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.MessageHandlers;

namespace EnergonSoftware.Core.Net
{
    public sealed class SessionManager
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SessionManager));

        private List<Session> _sessions = new List<Session>();
        private MessageProcessor _processor = new MessageProcessor();

        public long SessionTimeout { get; set; }

        public SessionManager()
        {
            SessionTimeout = -1;
        }

        public void Start(IMessageHandlerFactory factory)
        {
            _processor.Start(factory);
        }

        public void Stop()
        {
            _logger.Info("Closing all sessions...");
            _sessions.ForEach(s => s.Disconnect());
            _sessions.Clear();

            _processor.Stop();
        }

        public void AddSession(Session session)
        {
            session.Timeout = SessionTimeout;

            _sessions.Add(session);
            _logger.Info("Added new session " + session.Id);
        }

        public void Cleanup()
        {
            int count = _sessions.RemoveAll(s =>
                {
                    if(!s.Connecting && !s.Connected) {
                        _processor.RemoveSession(s.Id);
                        return true;
                    }
                    return false;
                }
            );

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
                session.Run(_processor);
            }
        }

        public void PollAndRun()
        {
            _sessions.ForEach(s => PollAndRun(s));
        }
    }
}
