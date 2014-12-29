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

        public static async Task<DatabaseConnection> AcquireDatabaseConnection(ConnectionStringSettings connectionSettings)
        {
            DatabaseConnection connection = new DatabaseConnection(connectionSettings);
            await connection.Open();
            return connection;
        }

        /*public static async Task<DatabaseConnection> AcquireDatabaseConnection()
        {
            return AcquireDatabaseConnection(ConfigurationManager.ConnectionStrings["energonsoftware"]);
        }*/

        public static bool TestDatabaseConnection()
        {
            return DatabaseConnection.TestDatabaseConnection(ConfigurationManager.ConnectionStrings["energonsoftware"]);
        }

#region Events Table
        private static async Task CreateEventsTables(DatabaseConnection connection)
        {
            Logger.Info("Creating event tables...");
            await AuthEvent.CreateTable(connection);
        }
#endregion

#region Accounts Table
        private static async Task CreateAccountsTables(DatabaseConnection connection)
        {
            Logger.Info("Creating account tables...");
            await AccountInfo.CreateTable(connection);
            await AccountFriend.CreateTable(connection);
        }

        private static async Task InsertAccountsData(DatabaseConnection connection)
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
            await shaneAccount.Insert(connection);
            Logger.Info("Inserted new account: " + shaneAccount);

            AccountInfo testAccount1 = new AccountInfo()
            {
                Active = true,
                Username = "test1",
            };
            testAccount1.SetPassword(authRealm, "password");
            await testAccount1.Insert(connection);
            Logger.Info("Inserted new account: " + testAccount1);

            AccountInfo testAccount2 = new AccountInfo()
            {
                Active = true,
                Username = "test2",
            };
            testAccount2.SetPassword(authRealm, "password");
            await testAccount2.Insert(connection);
            Logger.Info("Inserted new account: " + testAccount2);

            AccountFriend friend = new AccountFriend()
            {
                Account = testAccount1.Id,
                Friend = testAccount2.Id,
            };
            await friend.Insert(connection);
            Logger.Info("Inserted new account friend: " + friend);
        }

        private static async Task VerifyAccountsData(DatabaseConnection connection)
        {
            Logger.Info("Verifying account data...");

            AccountInfo account = new AccountInfo() { Username = "shane", };
            await account.Read(connection);
            Logger.Info("Read account: " + account);
        }
#endregion

#region Database
        public static async Task<bool> InitializeDatabase()
        {
            try {
                ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["energonsoftware"];

                Logger.Info("Creating database...");
                if(!(await CreateDatabase(connectionSettings))) {
                    Logger.Error("Failed to create database!");
                    return false;
                }

                Logger.Info("Populating database...");
                using(DatabaseConnection connection = await AcquireDatabaseConnection(connectionSettings)) {
                    await CreateEventsTables(connection);

                    await CreateAccountsTables(connection);
                    await InsertAccountsData(connection);
                    await VerifyAccountsData(connection);
                }

                return true;
            } catch(Exception e) {
                Logger.Error("Failed to create database!", e);
                return false;
            }
        }

        // TODO: this kind of assumes we're using SQLite and we shouldn't do that
        private static async Task<bool> CreateDatabase(ConnectionStringSettings connectionSettings)
        {
            string dataSource = DatabaseConnection.ParseDataSource(connectionSettings);
            if(File.Exists(dataSource)) {
                string backupfilename = dataSource + ".bak";
                Logger.Info("Backing up old database to " + backupfilename + "...");
                File.Delete(backupfilename);
                File.Move(dataSource, backupfilename);
            }
            return await DatabaseConnection.CreateDatabase(connectionSettings);
        }
    }
#endregion
}
