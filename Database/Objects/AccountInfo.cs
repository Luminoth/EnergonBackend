using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

using EnergonSoftware.Core;
using EnergonSoftware.Core.Account;
using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Database.Objects
{
    public sealed class AccountInfo : IDatabaseObject
    {
        private static TableDescription ACCOUNTS_TABLE = new TableDescription("accounts",
            new List<ColumnDescription>
            {
                { new ColumnDescription("id", DatabaseType.Integer).SetPrimaryKey() },
                { new ColumnDescription("active", DatabaseType.Boolean).SetNotNull() },
                { new ColumnDescription("username", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("passwordMD5", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("passwordSHA512", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("sessionid", DatabaseType.Text) },
                { new ColumnDescription("status", DatabaseType.Integer) },
            }
        );

        public static string TableName { get { return ACCOUNTS_TABLE.Name; } }

        public static void CreateTable(DatabaseConnection connection)
        {
            ACCOUNTS_TABLE.Create(connection);
        }

        public static List<Friend> ReadFriends(DatabaseConnection connection, long accountId)
        {
            List<Friend> friends = new List<Friend>();

            using(DbCommand command = connection.BuildCommand("SELECT id, username, status FROM " + ACCOUNTS_TABLE.Name + " WHERE id IN"
                + " (SELECT friend FROM " + AccountFriend.TableName + " where account=@account) AND active=1"))
            {
                connection.AddParameter(command, "account", accountId);
                using(DbDataReader reader = command.ExecuteReader()) {
                    while(reader.Read()) {
                        Friend friend = new Friend();
                        friend.Id = reader.GetInt32(0);
                        friend.Name = reader.GetString(1);
                        friend.Status = (Status)reader.GetInt32(2);
                        friends.Add(friend);
                    }
                }
            }

            return friends;
        }

#region Cleanliness
        private bool _dirty = false;

        public bool Dirty { get { return _dirty; } set { _dirty = value; } }

        public void Clean()
        {
            Dirty = false;
        }
#endregion

        private long _id = -1;
        private bool _active = false;
        private string _username;
        private string _passwordMD5;
        private string _passwordSHA512;
        private string _sessionid;
        private Status _status = Status.Offline;

        public long Id { get { return _id; } }
        public bool Valid { get { return _id > 0; } }

        public bool Active { get { return _active; } set { _active = value; _dirty = true; } }

        // NOTE: changing the username invalidates the password digest!
        public string Username { get { return _username; } set { _username = value; _dirty = true; } }

        public string PasswordMD5 { get { return _passwordMD5; } }
        public string PasswordSHA512 { get { return _passwordSHA512; } }

        public string SessionId { get { return _sessionid; } set { _sessionid = value; _dirty = true; } }

        public Status Status { get { return _status; } set { _status = value; _dirty = true; } }

        public AccountInfo()
        {
        }

        public AccountInfo(long id)
        {
            _id = id;
        }

        public AccountInfo(string username)
        {
            _username = username;
        }

        public void SetPassword(string realm, string password)
        {
            _passwordMD5 = new MD5().DigestPassword(Username, realm, password);
            _passwordSHA512 = new SHA512().DigestPassword(Username, realm, password);

            _dirty = true;
        }

        public bool Read(DatabaseConnection connection)
        {
            DbCommand command = null;
            if(Id > 0) {
                command = connection.BuildCommand("SELECT * FROM " + ACCOUNTS_TABLE.Name + " WHERE id=@id");
                connection.AddParameter(command, "id", Id);
            } else {
                command = connection.BuildCommand("SELECT * FROM " + ACCOUNTS_TABLE.Name + " WHERE username=@username");
                connection.AddParameter(command, "username", Username);
            }

            using(command) {
                using(DbDataReader reader = command.ExecuteReader()) {
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
            _id = reader.GetInt32(ACCOUNTS_TABLE["id"].Id);
            _active = reader.GetBoolean(ACCOUNTS_TABLE["active"].Id);
            _username = reader.GetString(ACCOUNTS_TABLE["username"].Id);
            _passwordMD5 = reader.GetString(ACCOUNTS_TABLE["passwordMD5"].Id);
            _passwordSHA512 = reader.GetString(ACCOUNTS_TABLE["passwordSHA512"].Id);

            if(!reader.IsDBNull(ACCOUNTS_TABLE["sessionid"].Id)) {
                _sessionid = reader.GetString(ACCOUNTS_TABLE["sessionid"].Id);
            }

            _status = (Status)reader.GetInt32(ACCOUNTS_TABLE["status"].Id);
        }

        public void Insert(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + ACCOUNTS_TABLE.Name
                + "(active, username" + ", passwordMD5" + ", passwordSHA512" + ", status)"
                + " VALUES(@active, @username" + ", @passwordMD5" + ", @passwordSHA512" + ", @status)"))
            {
                connection.AddParameter(command, "active", Active);
                connection.AddParameter(command, "username", Username);
                connection.AddParameter(command, "passwordMD5", PasswordMD5);
                connection.AddParameter(command, "passwordSHA512", PasswordSHA512);
                connection.AddParameter(command, "status", Status);
                command.ExecuteNonQuery();
                _id = connection.LastInsertRowId;
            }
        }

        public void Update(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("UPDATE " + ACCOUNTS_TABLE.Name
                + " SET active=@active, sessionid=@sessionid, status=@status WHERE id=@id"))
            {
                connection.AddParameter(command, "active", Active);
                connection.AddParameter(command, "sessionid", SessionId);
                connection.AddParameter(command, "status", Status);
                connection.AddParameter(command, "id", Id);
                command.ExecuteNonQuery();
            }
            Clean();
        }

        public void Delete(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + ACCOUNTS_TABLE.Name + " WHERE id=@id")) {
                connection.AddParameter(command, "id", Id);
                command.ExecuteNonQuery();
            }
        }

        public override string ToString()
        {
            return "Account(id: " + Id + ", active: " + Active
                + ", username: " + Username + ", passwordMD5: " + PasswordMD5 + ", passwordSHA512: " + PasswordSHA512
                + ", sessionid: " + SessionId + ", status: " + Status + ")";
        }
    }
}