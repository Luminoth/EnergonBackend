using System;
using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.Database;
using EnergonSoftware.Database.Models.Events;

using log4net;

namespace EnergonSoftware.Authenticator
{
    internal sealed class EventLogger
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EventLogger));

#region Singleton
        private static readonly EventLogger _instance = new EventLogger();
        public static EventLogger Instance { get { return _instance; } }
#endregion

        public async Task RequestEvent(EndPoint origin)
        {
            await LogEvent(new AuthEvent(AuthEventType.Request)
                {
                    Origin = origin.ToString(),
                }
            );
        }

        public async Task BeginEvent(EndPoint origin, string username)
        {
            await LogEvent(new AuthEvent(AuthEventType.Begin)
                {
                    Origin = origin.ToString(),
                    Account = username,
                }
            );
        }

        public async Task SuccessEvent(EndPoint origin, string username)
        {
            await LogEvent(new AuthEvent(AuthEventType.Success)
                {
                    Origin = origin.ToString(),
                    Account = username,
                }
            );
        }

        public async Task FailedEvent(EndPoint origin, string username, string reason)
        {
            await LogEvent(new AuthEvent(AuthEventType.Failure)
                {
                    Origin = origin.ToString(),
                    Account = username,
                    Reason = reason,
                }
            );
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
