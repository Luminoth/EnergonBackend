using System;
using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Events;

namespace EnergonSoftware.Overmind
{
    internal sealed class EventLogger
    {
        public static readonly EventLogger Instance = new EventLogger();

        public async Task StartupEventAsync()
        {
            await StartupEventAsync(new StartupEvent()
                {
                    Type = StartupEventType.Startup,
                    Application = Overmind.ServiceId,
                }).ConfigureAwait(false);
        }

        public async Task ShutdownEventAsync()
        {
            await StartupEventAsync(new StartupEvent()
                {
                    Type = StartupEventType.Shutdown,
                    Application = Overmind.ServiceId,
                }).ConfigureAwait(false);
        }

        private async Task StartupEventAsync(StartupEvent evt)
        {
            using(EventsDatabaseContext context = new EventsDatabaseContext()) {
                context.StartupEvents.Add(evt);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private EventLogger()
        {
        }
    }
}
