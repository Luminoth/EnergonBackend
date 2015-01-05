using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

using EnergonSoftware.Database;
using EnergonSoftware.Database.Models;
using EnergonSoftware.Database.Models.Events;

using log4net;

namespace EnergonSoftware.DbInit
{
    internal static class DatabaseManager
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(DatabaseManager));

        public static async Task<DatabaseConnection> AcquireDatabaseConnectionAsync(ConnectionStringSettings connectionSettings)
        {
            DatabaseConnection connection = new DatabaseConnection(connectionSettings);
            await connection.OpenAsync().ConfigureAwait(false);
            return connection;
        }

        /*public static async Task<DatabaseConnection> AcquireDatabaseConnectionAsync()
        {
            return await AcquireDatabaseConnectionAsync(ConfigurationManager.ConnectionStrings["energonsoftware"]).ConfigureAwait(false);
        }*/

        public static async Task<bool> TestDatabaseConnectionAsync()
        {
            return await DatabaseConnection.TestDatabaseConnectionAsync(ConfigurationManager.ConnectionStrings["energonsoftware"]).ConfigureAwait(false);
        }

#region Events Table
        private static async Task CreateEventsTablesAsync(DatabaseConnection connection)
        {
            Logger.Info("Creating event tables...");
            await AuthEvent.CreateTableAsync(connection).ConfigureAwait(false);
        }
#endregion

#region Accounts Table
        private static async Task CreateAccountsTablesAsync(DatabaseConnection connection)
        {
            Logger.Info("Creating account tables...");
            await AccountInfo.CreateTableAsync(connection).ConfigureAwait(false);
            await AccountFriend.CreateTableAsync(connection).ConfigureAwait(false);
        }

        private static async Task InsertAccountsDataAsync(DatabaseConnection connection)
        {
            Logger.Info("Inserting account data...");

            string authRealm = ConfigurationManager.AppSettings["authRealm"];
            Logger.Debug("Using authRealm='" + authRealm + "'");

            AccountInfo shaneAccount = new AccountInfo()
            {
                Active = true,
                Username = "shane",
            };
            shaneAccount.SetPassword(authRealm, "password");
            await shaneAccount.InsertAsync(connection).ConfigureAwait(false);
            Logger.Info("Inserted new account: " + shaneAccount);

            AccountInfo testAccount1 = new AccountInfo()
            {
                Active = true,
                Username = "test1",
            };
            testAccount1.SetPassword(authRealm, "password");
            await testAccount1.InsertAsync(connection).ConfigureAwait(false);
            Logger.Info("Inserted new account: " + testAccount1);

            AccountInfo testAccount2 = new AccountInfo()
            {
                Active = true,
                Username = "test2",
            };
            testAccount2.SetPassword(authRealm, "password");
            await testAccount2.InsertAsync(connection).ConfigureAwait(false);
            Logger.Info("Inserted new account: " + testAccount2);

            AccountFriend friend = new AccountFriend()
            {
                Account = testAccount1.Id,
                Friend = testAccount2.Id,
            };
            await friend.InsertAsync(connection).ConfigureAwait(false);
            Logger.Info("Inserted new account friend: " + friend);
        }

        private static async Task VerifyAccountsDataAsync(DatabaseConnection connection)
        {
            Logger.Info("Verifying account data...");

            AccountInfo account = new AccountInfo() { Username = "shane", };
            await account.ReadAsync(connection).ConfigureAwait(false);
            Logger.Info("Read account: " + account);
        }
#endregion

#region Database
        public static async Task<bool> InitializeDatabaseAsync()
        {
            try {
                ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["energonsoftware"];

                Logger.Info("Creating database...");
                if(!await CreateDatabaseAsync(connectionSettings).ConfigureAwait(false)) {
                    Logger.Error("Failed to create database!");
                    return false;
                }

                Logger.Info("Populating database...");
                using(DatabaseConnection connection = await AcquireDatabaseConnectionAsync(connectionSettings).ConfigureAwait(false)) {
                    await CreateEventsTablesAsync(connection).ConfigureAwait(false);

                    await CreateAccountsTablesAsync(connection).ConfigureAwait(false);
                    await InsertAccountsDataAsync(connection).ConfigureAwait(false);
                    await VerifyAccountsDataAsync(connection).ConfigureAwait(false);
                }

                return true;
            } catch(Exception e) {
                Logger.Error("Failed to create database!", e);
                return false;
            }
        }

        // TODO: this kind of assumes we're using SQLite and we shouldn't do that
        private static async Task<bool> CreateDatabaseAsync(ConnectionStringSettings connectionSettings)
        {
            string dataSource = DatabaseConnection.ParseDataSource(connectionSettings);
            if(File.Exists(dataSource)) {
                string backupfilename = dataSource + ".bak";
                Logger.Info("Backing up old database to " + backupfilename + "...");
                await Task.Run(() => File.Delete(backupfilename)).ConfigureAwait(false);
                await Task.Run(() => File.Move(dataSource, backupfilename)).ConfigureAwait(false);
            }
            return await DatabaseConnection.CreateDatabaseAsync(connectionSettings).ConfigureAwait(false);
        }
    }
#endregion
}
