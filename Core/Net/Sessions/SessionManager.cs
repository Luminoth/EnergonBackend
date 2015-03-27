using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.Core.Messages;

using log4net;

namespace EnergonSoftware.Core.Net.Sessions
{
    public sealed class SessionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SessionManager));

        private readonly object _lock = new object();
 
        private readonly List<Session> _sessions = new List<Session>();

        public int Count { get { return _sessions.Count; } }

        public void Add(Session session)
        {
            if(null == session) {
                throw new ArgumentNullException("session");
            }

            lock(_lock) {
                Logger.Info("Managing new session: " + session.Id);
                _sessions.Add(session);
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

        public async Task DisconnectAllAsync()
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                Logger.Info("Disconnecting all sessions...");
                _sessions.ForEach(session => tasks.Add(session.DisconnectAsync()));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            lock(_lock) {
                _sessions.Clear();
            }
        }

        public void Cleanup()
        {
            lock(_lock) {
                int count = _sessions.RemoveAll(session => !session.Connecting && !session.Connected);
                if(count > 0) {
                    Logger.Info("Removed " + count + " disconnected sessions");
                }
            }
        }

        public async Task PollAndRunAsync(int microSeconds)
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                _sessions.ForEach(session => tasks.Add(session.PollAndRunAsync(microSeconds)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task BroadcastMessageAsync(IMessage message)
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                _sessions.ForEach(session => tasks.Add(session.SendMessageAsync(message)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
