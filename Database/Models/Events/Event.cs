using System.Data.Common;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util;

namespace EnergonSoftware.Database.Models.Events
{
    public abstract class Event : IDatabaseObject
    {
#region Cleanliness
        public bool Dirty { get; set; }

        public void Clean()
        {
            Dirty = false;
        }
#endregion

        public long Id { get; protected set; }
        public long Timestamp { get; protected set; }

        protected Event()
        {
            Timestamp = Time.CurrentTimeMs;
        }

        public async Task<bool> ReadAsync(DatabaseConnection connection)
        {
            await Task.Delay(0).ConfigureAwait(false);
            return false;
        }

        public void Load(DbDataReader reader)
        {
        }

        public abstract Task InsertAsync(DatabaseConnection connection);

        public async Task UpdateAsync(DatabaseConnection connection)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task DeleteAsync(DatabaseConnection connection)
        {
            await Task.Delay(0).ConfigureAwait(false);
        }
    }
}
