using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

using EnergonSoftware.Core;
using EnergonSoftware.Database;
using EnergonSoftware.Database.Objects;
using EnergonSoftware.Database.Objects.Events;

using log4net;
using log4net.Config;

namespace EnergonSoftware.DbInit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, INotifyPropertyChanged
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(App));

        private bool _running = false;
        public bool Running { get { return _running; } private set { _running = value; NotifyPropertyChanged(); NotifyPropertyChanged("NotRunning"); } }
        public bool NotRunning { get { return !Running; } }

#region Initialization
        private void ConfigureLogging()
        {
            XmlConfigurator.Configure();
        }
#endregion

#region Database
        public Task<bool> InitializeDatabase()
        {
            return Task<bool>.Factory.StartNew(() =>
                {
                    Running = true;
                    try {
                        ConnectionStringSettings connectionSettings = ConfigurationManager.ConnectionStrings["energonsoftware"];

                        Logger.Info("Creating database...");
                        if(!CreateDatabase(connectionSettings)) {
                            Logger.Error("Failed to create database!");
                            return false;
                        }

                        Logger.Info("Populating database...");
                        using(DatabaseConnection connection = AcquireDatabaseConnection(connectionSettings)) {
                            CreateEventsTables(connection);

                            CreateAccountsTables(connection);
                            InsertAccountsData(connection);
                            VerifyAccountsData(connection);
                        }

                        return true;
                    } finally {
                        Running = false;
                    }
                }
            );
        }

        // TODO: this kind of assumes we're using SQLite and we shouldn't do that
        private bool CreateDatabase(ConnectionStringSettings connectionSettings)
        {
            string dataSource = DatabaseConnection.ParseDataSource(connectionSettings);
            if(File.Exists(dataSource)) {
                string backupfilename = dataSource + ".bak";
                Logger.Info("Backing up old database to " + backupfilename + "...");
                File.Delete(backupfilename);
                File.Move(dataSource, backupfilename);
            }
            return DatabaseConnection.CreateDatabase(connectionSettings);
        }

        private DatabaseConnection AcquireDatabaseConnection(ConnectionStringSettings connectionSettings)
        {
            DatabaseConnection connection = new DatabaseConnection(connectionSettings);
            connection.Open();
            return connection;
        }
#endregion

#region Events Table
        private void CreateEventsTables(DatabaseConnection connection)
        {
            Logger.Info("Creating event tables...");
            AuthEvent.CreateTable(connection);
            LoginEvent.CreateTable(connection);
        }
#endregion

#region Accounts Table
        private void CreateAccountsTables(DatabaseConnection connection)
        {
            Logger.Info("Creating account tables...");
            AccountInfo.CreateTable(connection);
            AccountFriend.CreateTable(connection);
        }

        private void InsertAccountsData(DatabaseConnection connection)
        {
            Logger.Info("Inserting account data...");

            string authRealm = ConfigurationManager.AppSettings["authRealm"];
            Logger.Debug("Using authRealm='" + authRealm + "'");

            AccountInfo shaneAccount = new AccountInfo();
            shaneAccount.Active = true;
            shaneAccount.Username = "shane";
            shaneAccount.SetPassword(authRealm, "password");
            shaneAccount.Insert(connection);
            Logger.Info("Inserted new account: " + shaneAccount);

            AccountInfo testAccount1 = new AccountInfo();
            testAccount1.Active = true;
            testAccount1.Username = "test1";
            testAccount1.SetPassword(authRealm, "password");
            testAccount1.Insert(connection);
            Logger.Info("Inserted new account: " + testAccount1);

            AccountFriend friend = new AccountFriend();
            friend.Account = shaneAccount.Id;
            friend.Friend = testAccount1.Id;
            friend.Insert(connection);
            Logger.Info("Inserted new account friend: " + friend);
        }

        private void VerifyAccountsData(DatabaseConnection connection)
        {
            Logger.Info("Verifying account data...");

            AccountInfo account = new AccountInfo("shane");
            account.Read(connection);
            Logger.Info("Read account: " + account);
        }
#endregion

#region Event Handlers
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();
        }
#endregion

#region Property Notifier
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string property=null)
        {
            if(null != PropertyChanged) {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
#endregion
    }
}
