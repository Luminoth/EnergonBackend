using System;
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

        public Task<bool> ReadAsync(DatabaseConnection connection)
        {
            throw new NotImplementedException("ReadAsync");
        }

        public Task LoadAsync(DbDataReader reader)
        {
            throw new NotImplementedException("LoadAsync");
        }

        public abstract Task InsertAsync(DatabaseConnection connection);

        public Task UpdateAsync(DatabaseConnection connection)
        {
            throw new NotImplementedException("UpdateAsync");
        }

        public Task DeleteAsync(DatabaseConnection connection)
        {
            throw new NotImplementedException("DeleteAsync");
        }
    }
}
