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

        public static readonly EventLogger Instance = new EventLogger();

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
