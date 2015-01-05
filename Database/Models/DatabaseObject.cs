using System.Data.Common;
using System.Threading.Tasks;

namespace EnergonSoftware.Database.Models
{
    public interface IDatabaseObject
    {
        bool Dirty { get; set; }
        void Clean();

        Task<bool> ReadAsync(DatabaseConnection connection);
        void Load(DbDataReader reader);

        Task InsertAsync(DatabaseConnection connection);
        Task UpdateAsync(DatabaseConnection connection);
        Task DeleteAsync(DatabaseConnection connection);
    }
}
