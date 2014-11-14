using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;

using log4net;

using EnergonSoftware.Core.MessageHandlers;

namespace EnergonSoftware.Core.Net
{
    public sealed class SessionManager
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SessionManager));

        private readonly object _lock = new object();

        private List<Session> _sessions = new List<Session>();
        public ReadOnlyCollection<Session> Sessions { get { lock(_lock) { return _sessions.AsReadOnly(); } } }

        private MessageProcessor _processor = new MessageProcessor();

        public bool Blocking { get; set; }
        public long SessionTimeout { get; set; }

        public SessionManager()
        {
            Blocking = true;
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

        public void AddSession(Session session)
        {
            lock(_lock) {
                session.Blocking = Blocking;
                session.Timeout = SessionTimeout;
                _sessions.Add(session);

                _logger.Info("Added new session " + session.Id);
            }
        }

        public void DisconnectAll()
        {
            lock(_lock) {
                _sessions.ForEach(s => s.Disconnect());
            }
        }

        public void Cleanup()
        {
            int count = 0;
            lock(_lock) {
                count = _sessions.RemoveAll(s =>
                    {
                        if(!s.Connecting && !s.Connected) {
                            _processor.RemoveSession(s.Id);
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

        private void PollAndRun(Session session)
        {
            int count = 0;
            if(!session.PollAndRead(out count)) {
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

        public void PollAndRun()
        {
            // NOTE: we don't use _sessions here so that
            // we work on a read-only copy and don't need
            // to lock the entire collection while we process
            foreach(Session session in Sessions) {
                PollAndRun(session);
            }
        }
    }
}
