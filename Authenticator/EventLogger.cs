using System;
using System.Net;

using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects.Events;

using log4net;

namespace EnergonSoftware.Authenticator
{
    internal sealed class EventLogger
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EventLogger));

#region Singleton
        private static EventLogger _instance = new EventLogger();
        public static EventLogger Instance { get { return _instance; } }
#endregion

        private readonly object _lock = new object();

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
                Logger.Debug("Logging event: " + evt);
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
