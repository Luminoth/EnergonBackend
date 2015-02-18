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

        public static readonly EventLogger Instance = new EventLogger();

        public async Task RequestEventAsync(EndPoint origin)
        {
            await LogEventAsync(new AuthEvent(AuthEventType.Request)
                {
                    Origin = origin.ToString(),
                }).ConfigureAwait(false);
        }

        public async Task BeginEventAsync(EndPoint origin, string account_name)
        {
            await LogEventAsync(new AuthEvent(AuthEventType.Begin)
                {
                    Origin = origin.ToString(),
                    AccountName = account_name,
                }).ConfigureAwait(false);
        }

        public async Task SuccessEventAsync(EndPoint origin, string account_name)
        {
            await LogEventAsync(new AuthEvent(AuthEventType.Success)
                {
                    Origin = origin.ToString(),
                    AccountName = account_name,
                }).ConfigureAwait(false);
        }

        public async Task FailedEventAsync(EndPoint origin, string account_name, string reason)
        {
            await LogEventAsync(new AuthEvent(AuthEventType.Failure)
                {
                    Origin = origin.ToString(),
                    AccountName = account_name,
                    Reason = reason,
                }).ConfigureAwait(false);
        }

        // TODO: move this to a base class
        private async Task LogEventAsync(Event evt)
        {
            Logger.Debug("Logging event: " + evt);
            using(DatabaseConnection connection = await DatabaseManager.AcquireDatabaseConnectionAsync().ConfigureAwait(false)) {
                await evt.InsertAsync(connection).ConfigureAwait(false);
            }
        }

        private EventLogger()
        {
        }
    }
}
