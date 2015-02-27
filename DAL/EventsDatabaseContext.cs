using System;
using System.Data.Entity;
using System.Threading.Tasks;

using EnergonSoftware.DAL.Models.Events;

using log4net;

namespace EnergonSoftware.DAL
{
    public class EventsDatabaseContext : DbContext
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(EventsDatabaseContext));

        public virtual DbSet<StartupEvent> StartupEvents { get; set; }
        public virtual DbSet<AuthenticateEvent> AuthenticationEvents { get; set; }

        public EventsDatabaseContext() : base("name=EnergonSoftwareEvents")
        {
            Database.Log = DebugLog;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try {
                await Database.Connection.OpenAsync();
                return true;
            } catch(Exception e) {
                Logger.Warn(e.ToString());
                return false;
            }
        }

        private void DebugLog(string sql)
        {
            Logger.Debug(sql);
        }
    }
}
