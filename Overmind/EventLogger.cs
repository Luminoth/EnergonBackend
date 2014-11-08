using System;
using System.Net;

using log4net;

using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects.Events;

namespace EnergonSoftware.Overmind
{
    sealed class EventLogger
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(EventLogger));

#region Singleton
        private static EventLogger _instance = new EventLogger();
        public static EventLogger Instance { get { return _instance; } }
#endregion

        private object _lock = new object();

        public void LoginRequestEvent(EndPoint origin, string username)
        {
            LoginEvent evt = new LoginEvent(LoginEventType.Request);
            evt.Origin = origin.ToString();
            evt.Account = origin.ToString();
            LogEvent(evt);
        }

        public void LoginSuccessEvent(EndPoint origin, string username)
        {
            LoginEvent evt = new LoginEvent(LoginEventType.Success);
            evt.Origin = origin.ToString();
            evt.Account = username;
            LogEvent(evt);
        }

        public void LoginFailedEvent(EndPoint origin, string username, string reason)
        {
            LoginEvent evt = new LoginEvent(LoginEventType.Failure);
            evt.Origin = origin.ToString();
            evt.Account = username;
            evt.Reason = reason;
            LogEvent(evt);
        }

        public void LogoutEvent(EndPoint origin, string username)
        {
            LoginEvent evt = new LoginEvent(LoginEventType.Logout);
            evt.Origin = origin.ToString();
            evt.Account = username;
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
