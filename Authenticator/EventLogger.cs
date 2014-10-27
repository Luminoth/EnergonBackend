using System;

using log4net;

using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects.Events;

namespace EnergonSoftware.Authenticator
{
    public sealed class EventLogger
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EventLogger));

#region Singleton
        private static EventLogger _instance = new EventLogger();
        public static EventLogger Instance { get { return _instance; } }
#endregion

        public void RequestEvent(string origin)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Request);
            evt.Origin = origin;
            LogEvent(evt);
        }

        public void BeginEvent(string origin, string username)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Begin);
            evt.Origin = origin;
            evt.Account = username;
            LogEvent(evt);
        }

        public void SuccessEvent(string origin, string username)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Success);
            evt.Origin = origin;
            evt.Account = username;
            LogEvent(evt);
        }

        public void FailedEvent(string origin, string username)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Failure);
            evt.Origin = origin;
            evt.Account = username;
            LogEvent(evt);
        }

        // TODO: move this to a base class
        private void LogEvent(Event evt)
        {
            _logger.Debug("Logging event: " + evt);
            using(DatabaseConnection connection = ServerState.Instance.AcquireDatabaseConnection()) {
                evt.Insert(connection);
            }
        }

        private EventLogger()
        {
        }
    }
}
