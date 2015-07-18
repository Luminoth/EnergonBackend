using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using log4net;

namespace EnergonSoftware.Core.Net.Sessions
{
    public class NetworkSessionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NetworkSessionManager));

        private readonly object _lock = new object();
 
        private readonly List<NetworkSession> _sessions = new List<NetworkSession>();

        public int Count { get { return _sessions.Count; } }

        public bool IsEmpty { get { return 0 == Count; } }

        public void Add(NetworkSession session)
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
                return _sessions.Exists(session => session.IsConnected && remoteEndpoint.Equals(session.RemoteEndPoint));
            }
        }

        public NetworkSession Get(EndPoint remoteEndpoint)
        {
            lock(_lock) {
                return _sessions.Find(session => session.IsConnected && remoteEndpoint.Equals(session.RemoteEndPoint));
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
                int count = _sessions.RemoveAll(session => !session.IsConnecting && !session.IsConnected);
                if(count > 0) {
                    Logger.Info("Removed " + count + " disconnected sessions");
                }
            }
        }

        public async Task PollAndReadAllAsync(int microSeconds)
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                _sessions.ForEach(session => tasks.Add(session.PollAndReadAllAsync(microSeconds)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task BroadcastAsync(byte[] data, int offset, int count)
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                _sessions.ForEach(session => tasks.Add(session.SendAsync(data, offset, count)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public async Task BroadcastAsync(MemoryStream buffer)
        {
            List<Task> tasks = new List<Task>();
            lock(_lock) {
                _sessions.ForEach(session => tasks.Add(session.SendAsync(buffer)));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
