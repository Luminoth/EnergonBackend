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
        private static readonly EventLogger _instance = new EventLogger();
        public static EventLogger Instance { get { return _instance; } }
#endregion

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
