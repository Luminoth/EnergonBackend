using System;
using System.Configuration;
using System.IO;
using System.Windows;

using log4net;
using log4net.Config;

using EnergonSoftware.Core;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;
using EnergonSoftware.Database.Objects.Events;

namespace EnergonSoftware.DbInit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Application));

        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        private static DatabaseConnection AcquireDatabaseConnection(ConnectionStringSettings connectionSettings)
        {
            DatabaseConnection connection = new DatabaseConnection(connectionSettings);
            connection.Open();

            return connection;
        }

        private static bool CreateDatabase(ConnectionStringSettings connectionSettings)
        {
            string dataSource = DatabaseConnection.ParseDataSource(connectionSettings);
            if(File.Exists(dataSource)) {
                string backupfilename = dataSource + ".bak";
                _logger.Info("Backing up old database to " + backupfilename + "...");
                File.Delete(backupfilename);
                File.Move(dataSource, backupfilename);
            }
            return DatabaseConnection.CreateDatabase(connectionSettings);
        }

        private static void CreateAccountsTables(DatabaseConnection connection)
        {
            _logger.Info("Creating event tables...");
            LoginEvent.CreateTable(connection);

            _logger.Info("Creating account tables...");
            AccountInfo.CreateTable(connection);
            AccountFriend.CreateTable(connection);
        }

        private static void InsertAccountsData(DatabaseConnection connection)
        {
            _logger.Info("Inserting account data...");

            AccountInfo shaneAccount = new AccountInfo();
            shaneAccount.Active = true;
            shaneAccount.Username = "shane";
            shaneAccount.SetPassword(Common.DEFAULT_AUTH_REALM, "password");
            shaneAccount.Insert(connection);
            _logger.Info("Inserted new account: " + shaneAccount);

            AccountInfo testAccount1 = new AccountInfo();
            testAccount1.Active = true;
            testAccount1.Username = "test1";
            testAccount1.SetPassword(Common.DEFAULT_AUTH_REALM, "password");
            testAccount1.Insert(connection);
            _logger.Info("Inserted new account: " + testAccount1);

            AccountFriend friend = new AccountFriend();
            friend.Account = shaneAccount.Id;
            friend.Friend = testAccount1.Id;
            friend.Insert(connection);
            _logger.Info("Inserted new account friend: " + friend);
        }

        private static void VerifyAccountsData(DatabaseConnection connection)
        {
            _logger.Info("Verifying account data...");

            AccountInfo account = new AccountInfo("shane");
            account.Read(connection);
            _logger.Info("Read account: " + account);
        }

        private static bool InitializeAccountsDatabase()
        {
            ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["accounts"];

            _logger.Info("Creating accounts database...");
            if(!CreateDatabase(connectionSettings)) {
                _logger.Error("Failed to create accounts database!");
                return false;
            }

            _logger.Info("Populating accounts database...");
            using(DatabaseConnection accountConnection = AcquireDatabaseConnection(connectionSettings)) {
                CreateAccountsTables(accountConnection);
                InsertAccountsData(accountConnection);
                VerifyAccountsData(accountConnection);
            }

            return true;
        }

        public static bool InitializeDatabases()
        {
            if(!InitializeAccountsDatabase()) {
                return false;
            }

            return true;
        }

#region Event Handlers
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();
        }
#endregion
    }
}
