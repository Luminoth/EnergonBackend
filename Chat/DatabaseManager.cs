using System;
using System.Configuration;
using System.Threading.Tasks;

using EnergonSoftware.Database;

namespace EnergonSoftware.Chat
{
    internal static class DatabaseManager
    {
        public static async Task<DatabaseConnection> AcquireDatabaseConnectionAsync()
        {
            DatabaseConnection connection = new DatabaseConnection(ConfigurationManager.ConnectionStrings["energonsoftware"]);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        public static async Task<bool> TestDatabaseConnectionAsync()
        {
            return await DatabaseConnection.TestDatabaseConnectionAsync(ConfigurationManager.ConnectionStrings["energonsoftware"]).ConfigureAwait(false);
        }
    }
}
