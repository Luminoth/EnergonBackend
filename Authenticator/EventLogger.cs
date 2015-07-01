using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Events;

namespace EnergonSoftware.Authenticator
{
    internal sealed class EventLogger
    {
        public static readonly EventLogger Instance = new EventLogger();

        public async Task StartupEventAsync()
        {
            await StartupEventAsync(new StartupEvent()
                {
                    Type = StartupEventType.Startup,
                    Application = Authenticator.ServiceId,
                }).ConfigureAwait(false);
        }

        public async Task ShutdownEventAsync()
        {
            await StartupEventAsync(new StartupEvent()
                {
                    Type = StartupEventType.Shutdown,
                    Application = Authenticator.ServiceId,
                }).ConfigureAwait(false);
        }

        public async Task RequestEventAsync(EndPoint origin)
        {
            await AuthenticateEventAsync(new AuthenticateEvent()
                {
                    Type = AuthenticateEventType.Request,
                    Origin = origin.ToString(),
                }).ConfigureAwait(false);
        }

        public async Task BeginEventAsync(EndPoint origin, string accountName)
        {
            await AuthenticateEventAsync(new AuthenticateEvent()
                {
                    Type = AuthenticateEventType.Begin,
                    Origin = origin.ToString(),
                    AccountName = accountName,
                }).ConfigureAwait(false);
        }

        public async Task SuccessEventAsync(EndPoint origin, string accountName)
        {
            await AuthenticateEventAsync(new AuthenticateEvent()
                {
                    Type = AuthenticateEventType.Success,
                    Origin = origin.ToString(),
                    AccountName = accountName,
                }).ConfigureAwait(false);
        }

        public async Task FailedEventAsync(EndPoint origin, string accountName, string reason)
        {
            await AuthenticateEventAsync(new AuthenticateEvent()
                {
                    Type = AuthenticateEventType.Failure,
                    Origin = origin.ToString(),
                    AccountName = accountName,
                    Reason = reason,
                }).ConfigureAwait(false);
        }

        private async Task StartupEventAsync(StartupEvent evt)
        {
            using(EventsDatabaseContext context = new EventsDatabaseContext()) {
                context.StartupEvents.Add(evt);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task AuthenticateEventAsync(AuthenticateEvent evt)
        {
            using(EventsDatabaseContext context = new EventsDatabaseContext()) {
                context.AuthenticationEvents.Add(evt);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private EventLogger()
        {
        }
    }
}
