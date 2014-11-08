using System;
using System.Net;

using log4net;

using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects.Events;

namespace EnergonSoftware.Authenticator
{
    sealed class EventLogger
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EventLogger));

#region Singleton
        private static EventLogger _instance = new EventLogger();
        public static EventLogger Instance { get { return _instance; } }
#endregion

        private object _lock = new object();

        public void RequestEvent(EndPoint origin)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Request);
            evt.Origin = origin.ToString();
            LogEvent(evt);
        }

        public void BeginEvent(EndPoint origin, string username)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Begin);
            evt.Origin = origin.ToString();
            evt.Account = username;
            LogEvent(evt);
        }

        public void SuccessEvent(EndPoint origin, string username)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Success);
            evt.Origin = origin.ToString();
            evt.Account = username;
            LogEvent(evt);
        }

        public void FailedEvent(EndPoint origin, string username, string reason)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Failure);
            evt.Origin = origin.ToString();
            evt.Account = username;
            evt.Reason = reason;
            LogEvent(evt);
        }

        // TODO: move this to a base class
        private void LogEvent(Event evt)
        {
            lock(_lock) {
                _logger.Debug("Logging event: " + evt);
                using(DatabaseConnection connection = DatabaseManager.AcquireDatabaseConnection()) {
                    evt.Insert(connection);
                }
            }
        }

        private EventLogger()
        {
        }
    }
}
