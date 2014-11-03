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
        private static readonly ILog _logger = LogManager.GetLogger(typeof(App));

#region Initialization
        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }

        public static bool InitializeDatabase()
        {
            ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["energonsoftware"];

            _logger.Info("Creating database...");
            if(!CreateDatabase(connectionSettings)) {
                _logger.Error("Failed to create database!");
                return false;
            }

            _logger.Info("Populating database...");
            using(DatabaseConnection connection = AcquireDatabaseConnection(connectionSettings)) {
                CreateEventsTables(connection);

                CreateAccountsTables(connection);
                InsertAccountsData(connection);
                VerifyAccountsData(connection);
            }

            return true;
        }
#endregion

#region Database
        // TODO: this kind of assumes we're using SQLite and we shouldn't do that
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

        private static DatabaseConnection AcquireDatabaseConnection(ConnectionStringSettings connectionSettings)
        {
            DatabaseConnection connection = new DatabaseConnection(connectionSettings);
            connection.Open();
            return connection;
        }
#endregion

#region Events Table
        private static void CreateEventsTables(DatabaseConnection connection)
        {
            _logger.Info("Creating event tables...");
            AuthEvent.CreateTable(connection);
            LoginEvent.CreateTable(connection);
        }
#endregion

#region Accounts Table
        private static void CreateAccountsTables(DatabaseConnection connection)
        {
            _logger.Info("Creating account tables...");
            AccountInfo.CreateTable(connection);
            AccountFriend.CreateTable(connection);
        }

        private static void InsertAccountsData(DatabaseConnection connection)
        {
            _logger.Info("Inserting account data...");

            string authRealm = ConfigurationManager.AppSettings["authRealm"];
            _logger.Debug("Using authRealm='" + authRealm + "'");

            AccountInfo shaneAccount = new AccountInfo();
            shaneAccount.Active = true;
            shaneAccount.Username = "shane";
            shaneAccount.SetPassword(authRealm, "password");
            shaneAccount.Insert(connection);
            _logger.Info("Inserted new account: " + shaneAccount);

            AccountInfo testAccount1 = new AccountInfo();
            testAccount1.Active = true;
            testAccount1.Username = "test1";
            testAccount1.SetPassword(authRealm, "password");
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
#endregion

#region Event Handlers
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();
        }
#endregion
    }
}
