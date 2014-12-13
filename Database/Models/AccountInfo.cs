using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Database.Models
{
    public sealed class AccountInfo : IDatabaseObject
    {
        private static readonly TableDescription AccountsTable = new TableDescription(
            "accounts",
            new List<ColumnDescription>
            {
                { new ColumnDescription("id", DatabaseType.Integer).SetPrimaryKey() },
                { new ColumnDescription("active", DatabaseType.Boolean).SetNotNull() },
                { new ColumnDescription("username", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("passwordMD5", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("passwordSHA512", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("endPoint", DatabaseType.Text) },
                { new ColumnDescription("sessionid", DatabaseType.Text) },
                { new ColumnDescription("visibility", DatabaseType.Integer).SetNotNull() },
                { new ColumnDescription("status", DatabaseType.Text) },
            }
        );

        public static string TableName { get { return AccountsTable.Name; } }

        public static async Task CreateTable(DatabaseConnection connection)
        {
            await AccountsTable.Create(connection);
        }

        public static async Task<List<Account>> ReadFriends(DatabaseConnection connection, long accountId)
        {
            List<Account> friends = new List<Account>();

            using(DbCommand command = connection.BuildCommand("SELECT id, username, visibility, status FROM " + AccountsTable.Name + " WHERE id IN"
                + " (SELECT friend FROM " + AccountFriend.TableName + " where account=@account) AND active=1"))
            {
                connection.AddParameter(command, "account", accountId);
                using(DbDataReader reader = await Task.Run(() => command.ExecuteReader())) {
                    while(reader.Read()) {
                        friends.Add(new Account()
                            {
                                Username = reader.GetString(1),
                                Visibility = (Visibility)reader.GetInt32(2),
                                Status = reader.GetString(3),
                            }
                        );
                    }
                }
            }

            return friends;
        }

#region Cleanliness
        public bool Dirty { get; set; }

        public void Clean()
        {
            Dirty = false;
        }
#endregion

        public long Id { get; private set; }
        public bool Valid { get { return Id > 0; } }

        private bool _active;
        public bool Active { get { return _active; } set { _active = value; Dirty = true; } }

        // NOTE: changing the username invalidates the password digest!
        private string _username;
        public string Username { get { return _username; } set { _username = value; Dirty = true; } }

        public string PasswordMD5 { get; private set; }
        public string PasswordSHA512 { get; private set; }

        private string _endPoint;
        public string EndPoint { get { return _endPoint; } set { _endPoint = value; Dirty = true; } }

        private string _sessionid;
        public string SessionId { get { return _sessionid; } set { _sessionid = value; Dirty = true; } }

        private Visibility _visibility = Visibility.Offline;
        public Visibility Visibility { get { return _visibility; } set { _visibility = value; Dirty = true; } }

        private string _status;
        public string Status { get { return _status; } set { _status = value; Dirty = true; } }

        public AccountInfo()
        {
            Id = -1;
        }

        public void SetPassword(string realm, string password)
        {
            PasswordMD5 = new MD5().DigestPassword(Username, realm, password);
            PasswordSHA512 = new SHA512().DigestPassword(Username, realm, password);

            Dirty = true;
        }

        public async Task<bool> Read(DatabaseConnection connection)
        {
            DbCommand command = null;
            if(Id > 0) {
                command = connection.BuildCommand("SELECT * FROM " + AccountsTable.Name + " WHERE id=@id");
                connection.AddParameter(command, "id", Id);
            } else {
                command = connection.BuildCommand("SELECT * FROM " + AccountsTable.Name + " WHERE username=@username");
                connection.AddParameter(command, "username", Username);
            }

            using(command) {
                using(DbDataReader reader = await Task.Run(() =>command.ExecuteReader())) {
                    if(!reader.Read()) {
                        return false;
                    }
                    Load(reader);
                }
            }

            Clean();
            return true;
        }

        public void Load(DbDataReader reader)
        {
            Id = reader.GetInt32(AccountsTable["id"].Id);
            _active = reader.GetBoolean(AccountsTable["active"].Id);
            _username = reader.GetString(AccountsTable["username"].Id);
            PasswordMD5 = reader.GetString(AccountsTable["passwordMD5"].Id);
            PasswordSHA512 = reader.GetString(AccountsTable["passwordSHA512"].Id);

            if(!reader.IsDBNull(AccountsTable["endPoint"].Id)) {
                _endPoint = reader.GetString(AccountsTable["endPoint"].Id);
            }

            if(!reader.IsDBNull(AccountsTable["sessionid"].Id)) {
                _sessionid = reader.GetString(AccountsTable["sessionid"].Id);
            }

            _visibility = (Visibility)reader.GetInt32(AccountsTable["visibility"].Id);

            if(!reader.IsDBNull(AccountsTable["status"].Id)) {
                _status = reader.GetString(AccountsTable["status"].Id);
            }
        }

        public async Task Insert(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + AccountsTable.Name
                + "(active, username, passwordMD5, passwordSHA512, visibility, status)"
                + " VALUES(@active, @username, @passwordMD5, @passwordSHA512, @visibility, @status)"))
            {
                connection.AddParameter(command, "active", Active);
                connection.AddParameter(command, "username", Username);
                connection.AddParameter(command, "passwordMD5", PasswordMD5);
                connection.AddParameter(command, "passwordSHA512", PasswordSHA512);
                connection.AddParameter(command, "visibility", Visibility);
                connection.AddParameter(command, "status", Status);
                await Task.Run(() => command.ExecuteNonQuery());
                Id = connection.LastInsertRowId;
            }
        }

        public async Task Update(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("UPDATE " + AccountsTable.Name
                + " SET active=@active, endPoint=@endPoint, sessionid=@sessionid, visibility=@visibility, status=@status WHERE id=@id"))
            {
                connection.AddParameter(command, "active", Active);
                connection.AddParameter(command, "endPoint", EndPoint);
                connection.AddParameter(command, "sessionid", SessionId);
                connection.AddParameter(command, "visibility", Visibility);
                connection.AddParameter(command, "status", Status);
                connection.AddParameter(command, "id", Id);
                await Task.Run(() => command.ExecuteNonQuery());
            }
            Clean();
        }

        public async Task Delete(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + AccountsTable.Name + " WHERE id=@id")) {
                connection.AddParameter(command, "id", Id);
                await Task.Run(() => command.ExecuteNonQuery());
            }
        }

        public Account ToAccount()
        {
            string[] endPoint = EndPoint.Split(':');
            if(2 != endPoint.Length) {
                throw new FormatException("Invalid EndPoint!");
            }

            return new Account()
            {
                Id = Id,
                Username = Username,
                SessionId = SessionId,
                EndPoint = new IPEndPoint(IPAddress.Parse(endPoint[0]), Convert.ToInt32(endPoint[1])),
                Visibility = Visibility,
            };
        }

        public override string ToString()
        {
            return "Account(id: " + Id + ", active: " + Active
                + ", username: " + Username + ", passwordMD5: " + PasswordMD5 + ", passwordSHA512: " + PasswordSHA512
                + ", endPoint: " + EndPoint + ", sessionid: " + SessionId + ", visibility: " + Visibility + ", status: " + Status + ")";
        }
    }
}