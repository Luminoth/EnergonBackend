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
        public async Task<bool> InitializeDatabase()
        {
            Running = true;
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
            } finally {
                Running = false;
            }
        }

        // TODO: this kind of assumes we're using SQLite and we shouldn't do that
        private async Task<bool> CreateDatabase(ConnectionStringSettings connectionSettings)
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

        private async Task<DatabaseConnection> AcquireDatabaseConnection(ConnectionStringSettings connectionSettings)
        {
            DatabaseConnection connection = new DatabaseConnection(connectionSettings);
            await connection.Open();
            return connection;
        }
#endregion

#region Events Table
        private async Task CreateEventsTables(DatabaseConnection connection)
        {
            Logger.Info("Creating event tables...");
            await AuthEvent.CreateTable(connection);
            await LoginEvent.CreateTable(connection);
        }
#endregion

#region Accounts Table
        private async Task CreateAccountsTables(DatabaseConnection connection)
        {
            Logger.Info("Creating account tables...");
            await AccountInfo.CreateTable(connection);
            await AccountFriend.CreateTable(connection);
        }

        private async Task InsertAccountsData(DatabaseConnection connection)
        {
            Logger.Info("Inserting account data...");

            string authRealm = ConfigurationManager.AppSettings["authRealm"];
            Logger.Debug("Using authRealm='" + authRealm + "'");

            AccountInfo shaneAccount = new AccountInfo();
            shaneAccount.Active = true;
            shaneAccount.Username = "shane";
            shaneAccount.SetPassword(authRealm, "password");
            await shaneAccount.Insert(connection);
            Logger.Info("Inserted new account: " + shaneAccount);

            AccountInfo testAccount1 = new AccountInfo();
            testAccount1.Active = true;
            testAccount1.Username = "test1";
            testAccount1.SetPassword(authRealm, "password");
            await testAccount1.Insert(connection);
            Logger.Info("Inserted new account: " + testAccount1);

            AccountFriend friend = new AccountFriend();
            friend.Account = shaneAccount.Id;
            friend.Friend = testAccount1.Id;
            await friend.Insert(connection);
            Logger.Info("Inserted new account friend: " + friend);
        }

        private async Task VerifyAccountsData(DatabaseConnection connection)
        {
            Logger.Info("Verifying account data...");

            AccountInfo account = new AccountInfo("shane");
            await account.Read(connection);
            Logger.Info("Read account: " + account);
        }
#endregion

#region Event Handlers
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ConfigureLogging();
            Common.InitFilesystem();

            /*if(!DatabaseConnection.TestDatabaseConnection(ConfigurationManager.ConnectionStrings["energonsoftware"])) {
                Logger.Fatal("Could not connect to database!");
                MessageBox.Show("Could not connect to database!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }*/
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
