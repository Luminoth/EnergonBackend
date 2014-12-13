using System;
using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.Database;
using EnergonSoftware.Database.Models.Events;

using log4net;

namespace EnergonSoftware.Overmind
{
    internal sealed class EventLogger
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EventLogger));

#region Singleton
        private static EventLogger _instance = new EventLogger();
        public static EventLogger Instance { get { return _instance; } }
#endregion

        public async Task LoginRequestEvent(EndPoint origin, string username)
        {
            LoginEvent evt = new LoginEvent(LoginEventType.Request);
            evt.Origin = origin.ToString();
            evt.Account = origin.ToString();
            await LogEvent(evt);
        }

        public async Task LoginSuccessEvent(EndPoint origin, string username)
        {
            LoginEvent evt = new LoginEvent(LoginEventType.Success);
            evt.Origin = origin.ToString();
            evt.Account = username;
            await LogEvent(evt);
        }

        public async Task LoginFailedEvent(EndPoint origin, string username, string reason)
        {
            LoginEvent evt = new LoginEvent(LoginEventType.Failure);
            evt.Origin = origin.ToString();
            evt.Account = username;
            evt.Reason = reason;
            await LogEvent(evt);
        }

        public async Task LogoutEvent(EndPoint origin, string username)
        {
            LoginEvent evt = new LoginEvent(LoginEventType.Logout);
            evt.Origin = origin.ToString();
            evt.Account = username;
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
