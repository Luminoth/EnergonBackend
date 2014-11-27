using System;
using System.Net;
using System.Threading.Tasks;

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

        public async Task RequestEvent(EndPoint origin)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Request);
            evt.Origin = origin.ToString();
            await LogEvent(evt);
        }

        public async Task BeginEvent(EndPoint origin, string username)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Begin);
            evt.Origin = origin.ToString();
            evt.Account = username;
            await LogEvent(evt);
        }

        public async Task SuccessEvent(EndPoint origin, string username)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Success);
            evt.Origin = origin.ToString();
            evt.Account = username;
            await LogEvent(evt);
        }

        public async Task FailedEvent(EndPoint origin, string username, string reason)
        {
            AuthEvent evt = new AuthEvent(AuthEventType.Failure);
            evt.Origin = origin.ToString();
            evt.Account = username;
            evt.Reason = reason;
            await LogEvent(evt);
        }

        // TODO: move this to a base class
        private async Task LogEvent(Event evt)
        {
            Logger.Debug("Logging event: " + evt);
            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnection()) {
                await evt.Insert(connection);
            }
        }

        private EventLogger()
        {
        }
    }
}
