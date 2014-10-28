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
                { new ColumnDescription("sessionEndPoint", DatabaseType.Text) },
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
        public bool Dirty { get; set; }

        public void Clean()
        {
            Dirty = false;
        }
#endregion

        public long Id { get; private set; }
        public bool Valid { get { return Id > 0; } }

        private bool _active = false;
        public bool Active { get { return _active; } set { _active = value; Dirty = true; } }

        // NOTE: changing the username invalidates the password digest!
        private string _username;
        public string Username { get { return _username; } set { _username = value; Dirty = true; } }

        public string PasswordMD5 { get; private set; }
        public string PasswordSHA512 { get; private set; }

        private string _sessionEndPoint;
        public string SessionEndPoint { get { return _sessionEndPoint; } set { _sessionEndPoint = value; Dirty = true; } }

        private string _sessionid;
        public string SessionId { get { return _sessionid; } set { _sessionid = value; Dirty = true; } }

        private Status _status = Status.Offline;
        public Status Status { get { return _status; } set { _status = value; Dirty = true; } }

        public AccountInfo()
        {
            Id = -1;
        }

        public AccountInfo(long id)
        {
            Id = id;
        }

        public AccountInfo(string username)
        {
            Id = -1;
            _username = username;
        }

        public void SetPassword(string realm, string password)
        {
            PasswordMD5 = new MD5().DigestPassword(Username, realm, password);
            PasswordSHA512 = new SHA512().DigestPassword(Username, realm, password);

            Dirty = true;
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
            Id = reader.GetInt32(ACCOUNTS_TABLE["id"].Id);
            _active = reader.GetBoolean(ACCOUNTS_TABLE["active"].Id);
            _username = reader.GetString(ACCOUNTS_TABLE["username"].Id);
            PasswordMD5 = reader.GetString(ACCOUNTS_TABLE["passwordMD5"].Id);
            PasswordSHA512 = reader.GetString(ACCOUNTS_TABLE["passwordSHA512"].Id);

            if(!reader.IsDBNull(ACCOUNTS_TABLE["sessionEndPoint"].Id)) {
                _sessionEndPoint = reader.GetString(ACCOUNTS_TABLE["sessionEndPoint"].Id);
            }

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
                Id = connection.LastInsertRowId;
            }
        }

        public void Update(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("UPDATE " + ACCOUNTS_TABLE.Name
                + " SET active=@active, sessionEndPoint=@sessionEndPoint, sessionid=@sessionid, status=@status WHERE id=@id"))
            {
                connection.AddParameter(command, "active", Active);
                connection.AddParameter(command, "sessionEndPoint", SessionEndPoint);
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
                + ", sessionEndPoint: " + SessionEndPoint + ", sessionid: " + SessionId + ", status: " + Status + ")";
        }
    }
}