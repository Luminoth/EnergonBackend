using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using EnergonSoftware.Core.Properties;
using EnergonSoftware.Core.Net.Sockets;

using log4net;

namespace EnergonSoftware.Core.Net.Sessions
{
    /// <summary>
    /// Manages a set of NetworkSessions.
    /// </summary>
    public class NetworkSessionManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NetworkSessionManager));

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
 
        private readonly List<NetworkSession> _sessions = new List<NetworkSession>();

        /// <summary>
        /// Gets or sets the maximum number of sessions.
        /// For an unlimited number of sessions, set to -1.
        /// </summary>
        /// <value>
        /// The maximum number of sessions.
        /// </value>
        public int MaxSessions { get; set; } = -1;

        /// <summary>
        /// Gets the number of sessions being managed.
        /// </summary>
        /// <value>
        /// The number of sessions being managed.
        /// </value>
        public int Count => _sessions.Count;

        /// <summary>
        /// Gets a value indicating whether this manager is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this manager is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => 0 == Count;

        /// <summary>
        /// Gets or sets the session factory.
        /// </summary>
        /// <value>
        /// The session factory.
        /// </value>
        public INetworkSessionFactory SessionFactory { private get; set; }

        /// <summary>
        /// New connection event handler.
        /// Connect this to one of the Listener classes.
        /// Using this method, there is no way of knowing whether or not the session was successfully added.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The <see cref="NewConnectionEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.InvalidOperationException">SessionFactory is null!</exception>
        public async void NewConnectionEventHandlerAsync(object sender, NewConnectionEventArgs e)
        {
            if(null == SessionFactory) {
                throw new InvalidOperationException("SessionFactory is null!");
            }

            NetworkSession session = SessionFactory.Create(e.Socket);
            await AddAsync(session).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>true if the session was added, false if the session limit would have been exceeded.</returns>
        /// <exception cref="System.ArgumentNullException">session</exception>
        public async Task<bool> AddAsync(NetworkSession session)
        {
            if(null == session) {
                throw new ArgumentNullException(nameof(session));
            }

            if(MaxSessions >= 0 && Count >= MaxSessions) {
                Logger.Info("Max connections exceeded, denying new connection!");
                await session.DisconnectAsync(Resources.ErrorConnectionLimitExceeded).ConfigureAwait(false);
                return false;
            }

            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                Logger.Info($"Managing new session: {session.Id}");
                _sessions.Add(session);
            } finally {
                _lock.Release();
            }

            return true;
        }

        /// <summary>
        /// Determines whether the manager contains a session connected to the specified remote endpoint.
        /// </summary>
        /// <param name="remoteEndpoint">The remote endpoint.</param>
        /// <returns>True if the manager contains a session connected to the specified remote endpoint</returns>
        public async Task<bool> ContainsAsync(EndPoint remoteEndpoint)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                return _sessions.Exists(session => session.IsConnected && remoteEndpoint.Equals(session.RemoteEndPoint));
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Gets the session connected to the specified remote endpoint, if one exists.
        /// </summary>
        /// <param name="remoteEndpoint">The remote endpoint.</param>
        /// <returns>The session connected to the specified remote endpoint, if one exists</returns>
        public async Task<NetworkSession> GetAsync(EndPoint remoteEndpoint)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                return _sessions.Find(session => session.IsConnected && remoteEndpoint.Equals(session.RemoteEndPoint));
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Disconnects all sessions.
        /// </summary>
        public async Task DisconnectAllAsync()
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                List<Task> tasks = new List<Task>();

                Logger.Info("Disconnecting all sessions...");
                _sessions.ForEach(session => tasks.Add(session.DisconnectAsync()));

                await Task.WhenAll(tasks).ConfigureAwait(false);

                _sessions.Clear();
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Removes all disconnected sessions.
        /// </summary>
        public async Task CleanupAsync()
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                int count = _sessions.RemoveAll(session => !session.IsConnecting && !session.IsConnected);
                if(count > 0) {
                    Logger.Info($"Removed {count} disconnected sessions");
                }
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Polls each session and reads all available data.
        /// </summary>
        /// <param name="microSeconds">The microsecond poll timeout.</param>
        public async Task PollAndReadAllAsync(int microSeconds)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                List<Task> tasks = new List<Task>();
                _sessions.ForEach(session => tasks.Add(session.PollAndReadAllAsync(microSeconds)));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Broadcasts data to all sessions.
        /// </summary>
        /// <param name="data">The data.</param>
        public async Task BroadcastAsync(byte[] data)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                List<Task> tasks = new List<Task>();
                _sessions.ForEach(session => tasks.Add(session.SendAsync(data)));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }

        /// <summary>
        /// Broadcasts data to all sessions.
        /// </summary>
        /// <param name="buffer">The data buffer.</param>
        public async Task BroadcastAsync(MemoryStream buffer)
        {
            await _lock.WaitAsync().ConfigureAwait(false);
            try {
                List<Task> tasks = new List<Task>();
                _sessions.ForEach(session => tasks.Add(session.SendAsync(buffer)));

                await Task.WhenAll(tasks).ConfigureAwait(false);
            } finally {
                _lock.Release();
            }
        }
    }
}
