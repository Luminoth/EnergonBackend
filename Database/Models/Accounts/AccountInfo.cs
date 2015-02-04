using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.Core.Accounts;
using EnergonSoftware.Core.Util.Crypt;

namespace EnergonSoftware.Database.Models.Accounts
{
    public sealed class AccountInfo : IDatabaseObject
    {
        private static readonly TableDescription AccountsTable = new TableDescription(
            "accounts",
            new List<ColumnDescription>
            {
                { new ColumnDescription("id", DatabaseType.Integer).SetPrimaryKey() },
                { new ColumnDescription("active", DatabaseType.Boolean).SetNotNull() },
                { new ColumnDescription("username", DatabaseType.Text).SetNotNull().SetUnique() },
                { new ColumnDescription("passwordMD5", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("passwordSHA512", DatabaseType.Text).SetNotNull() },
                { new ColumnDescription("endPoint", DatabaseType.Text) },
                { new ColumnDescription("sessionid", DatabaseType.Text) },
                { new ColumnDescription("visibility", DatabaseType.Integer).SetNotNull() },
                { new ColumnDescription("status", DatabaseType.Text) },
            }
        );

        public static string TableName { get { return AccountsTable.Name; } }

        public static async Task CreateTableAsync(DatabaseConnection connection)
        {
            await AccountsTable.CreateAsync(connection).ConfigureAwait(false);
        }

        public static async Task<List<Account>> ReadFriendsAsync(DatabaseConnection connection, long accountId)
        {
            List<Account> friends = new List<Account>();

            using(DbCommand command = connection.BuildCommand("SELECT a.username, a.visibility, a.status, fg.name as group_name"
                + " FROM " + TableName + " a, " + AccountFriend.TableName + " af"
                    + " LEFT JOIN " + FriendGroup.TableName + " fg ON af.group_id = fg.id"
                + " WHERE a.id = af.friend_account_id AND af.account_id=@account_id AND a.active=1"))
            {
                connection.AddParameter(command, "account_id", accountId);
                using(DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false)) {
                    while(await reader.ReadAsync().ConfigureAwait(false)) {
                        Account account = new Account()
                        {
                            Username = reader.GetString(0),
                            Visibility = (Visibility)reader.GetInt32(1),
                        };

                        if(!await reader.IsDBNullAsync(2).ConfigureAwait(false)) {
                            account.Status = reader.GetString(2);
                        }

                        if(!await reader.IsDBNullAsync(3).ConfigureAwait(false)) {
                            account.Group = reader.GetString(3);
                        }

                        friends.Add(account);
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
        private string _username = string.Empty;
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
            PasswordMD5 = string.Empty;
            PasswordSHA512 = string.Empty;
        }

        public async Task SetPassword(string realm, string password)
        {
            PasswordMD5 = await new MD5().DigestPasswordAsync(Username, realm, password).ConfigureAwait(false);
            PasswordSHA512 = await new SHA512().DigestPasswordAsync(Username, realm, password).ConfigureAwait(false);

            Dirty = true;
        }

        public async Task<bool> ReadAsync(DatabaseConnection connection)
        {
            DbCommand command = null;
            if(Id > 0) {
                command = connection.BuildCommand("SELECT * FROM " + TableName + " WHERE id=@id");
                connection.AddParameter(command, "id", Id);
            } else {
                command = connection.BuildCommand("SELECT * FROM " + TableName + " WHERE username=@username");
                connection.AddParameter(command, "username", Username);
            }

            using(command) {
                using(DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false)) {
                    if(!await reader.ReadAsync().ConfigureAwait(false)) {
                        return false;
                    }
                    await LoadAsync(reader).ConfigureAwait(false);
                }
            }

            Clean();
            return true;
        }

        public async Task LoadAsync(DbDataReader reader)
        {
            if(null == reader) {
                throw new ArgumentNullException("reader");
            }

            Id = reader.GetInt32(AccountsTable["id"].Id);
            _active = reader.GetBoolean(AccountsTable["active"].Id);
            _username = reader.GetString(AccountsTable["username"].Id);
            PasswordMD5 = reader.GetString(AccountsTable["passwordMD5"].Id);
            PasswordSHA512 = reader.GetString(AccountsTable["passwordSHA512"].Id);

            if(!await reader.IsDBNullAsync(AccountsTable["endPoint"].Id).ConfigureAwait(false)) {
                _endPoint = reader.GetString(AccountsTable["endPoint"].Id);
            }

            if(!await reader.IsDBNullAsync(AccountsTable["sessionid"].Id).ConfigureAwait(false)) {
                _sessionid = reader.GetString(AccountsTable["sessionid"].Id);
            }

            _visibility = (Visibility)reader.GetInt32(AccountsTable["visibility"].Id);

            if(!await reader.IsDBNullAsync(AccountsTable["status"].Id).ConfigureAwait(false)) {
                _status = reader.GetString(AccountsTable["status"].Id);
            }
        }

        public async Task InsertAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + TableName
                + "(active, username, passwordMD5, passwordSHA512, visibility, status)"
                + " VALUES(@active, @username, @passwordMD5, @passwordSHA512, @visibility, @status)"))
            {
                connection.AddParameter(command, "active", Active);
                connection.AddParameter(command, "username", Username);
                connection.AddParameter(command, "passwordMD5", PasswordMD5);
                connection.AddParameter(command, "passwordSHA512", PasswordSHA512);
                connection.AddParameter(command, "visibility", Visibility);
                connection.AddParameter(command, "status", Status);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                Id = connection.LastInsertRowId;
            }
        }

        public async Task UpdateAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("UPDATE " + TableName
                + " SET active=@active, endPoint=@endPoint, sessionid=@sessionid, visibility=@visibility, status=@status WHERE id=@id"))
            {
                connection.AddParameter(command, "active", Active);
                connection.AddParameter(command, "endPoint", EndPoint);
                connection.AddParameter(command, "sessionid", SessionId);
                connection.AddParameter(command, "visibility", Visibility);
                connection.AddParameter(command, "status", Status);
                connection.AddParameter(command, "id", Id);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            Clean();
        }

        public async Task DeleteAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + TableName + " WHERE id=@id")) {
                connection.AddParameter(command, "id", Id);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public Account ToAccount()
        {
            EndPoint endPoint = null;
            if(null != EndPoint) {
                string[] endPointStr = EndPoint.Split(':');
                if(2 != endPointStr.Length) {
                    throw new FormatException("Invalid EndPoint!");
                }
                endPoint = new IPEndPoint(IPAddress.Parse(endPointStr[0]), Convert.ToInt32(endPointStr[1]));
            } 

            return new Account()
            {
                Id = Id,
                Username = Username,
                SessionId = SessionId,
                EndPoint = endPoint,
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