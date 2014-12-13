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

        public Event()
        {
            Timestamp = Time.CurrentTimeMs;
        }

        public async Task<bool> Read(DatabaseConnection connection)
        {
            await Task.Delay(0);
            return false;
        }

        public void Load(DbDataReader reader)
        {
        }

        public abstract Task Insert(DatabaseConnection connection);

        public async Task Update(DatabaseConnection connection)
        {
            await Task.Delay(0);
        }

        public async Task Delete(DatabaseConnection connection)
        {
            await Task.Delay(0);
        }
    }
}
