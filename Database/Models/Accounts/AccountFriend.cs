using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace EnergonSoftware.Database.Models.Accounts
{
    public sealed class AccountFriend : IDatabaseObject
    {
        private static readonly TableDescription AccountFriendsTable = new TableDescription(
            "accounts_friends",
            new List<ColumnDescription>
            {
                { new ColumnDescription("account_id", DatabaseType.Integer).SetPrimaryKey().SetReferences("accounts", "id") },
                { new ColumnDescription("friend_account_id", DatabaseType.Integer).SetPrimaryKey().SetReferences("accounts", "id") },
                { new ColumnDescription("group_id", DatabaseType.Integer).SetDefaultValue("-1").SetReferences("friend_groups", "id") },
            });

        public static string TableName { get { return AccountFriendsTable.Name; } }

        public static async Task CreateTableAsync(DatabaseConnection connection)
        {
            await AccountFriendsTable.CreateAsync(connection).ConfigureAwait(false);
        }

        // NOTE: deletes both accountId -> friend and friend -> accountId mappings
        public static async Task DeleteAllAsync(DatabaseConnection connection, long accountId)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + TableName + " WHERE account_id=@account_id OR friend_account_id=@friend_account_id")) {
                connection.AddParameter(command, "account_id", accountId);
                connection.AddParameter(command, "friend_account_id", accountId);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

#region Cleanliness
        public bool Dirty { get; set; }

        public void Clean()
        {
            Dirty = false;
        }
#endregion

        private long _account_id = -1;
        public long AccountId
        {
            get { return _account_id; }

            set {
                _account_id = value;
                Dirty = true;
            }
        }

        private long _friend_account_id = -1;
        public long FriendAccountId
        {
            get { return _friend_account_id; }

            set {
                _friend_account_id = value;
                Dirty = true;
            }
        }

        private long _group_id = -1;
        public long GroupId
        {
            get { return _group_id; }

            set {
                _group_id = value;
                Dirty = true;
            }
        }

        public AccountFriend()
        {
        }

        public Task<bool> ReadAsync(DatabaseConnection connection)
        {
            throw new NotImplementedException("ReadAsync");
        }

        public async Task LoadAsync(DbDataReader reader)
        {
            if(null == reader) {
                throw new ArgumentNullException("reader");
            }

            _account_id = reader.GetInt32(AccountFriendsTable["account_id"].Id);
            _friend_account_id = reader.GetInt32(AccountFriendsTable["friend_account_id"].Id);
            _group_id = reader.GetInt32(AccountFriendsTable["group_id"].Id);

            await Task.Delay(0).ConfigureAwait(false);
        }

        public async Task InsertAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + TableName
                + "(account_id, friend_account_id, group_id) VALUES(@account_id, @friend_account_id, @group_id)"))
            {
                connection.AddParameter(command, "account_id", AccountId);
                connection.AddParameter(command, "friend_account_id", FriendAccountId);
                connection.AddParameter(command, "group_id", GroupId);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public Task UpdateAsync(DatabaseConnection connection)
        {
            throw new NotImplementedException("UpdateAsync");
        }

        public async Task DeleteAsync(DatabaseConnection connection)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + TableName + " WHERE account_id=@account_id AND friend_account_id=@friend_account_id")) {
                connection.AddParameter(command, "account_id", AccountId);
                connection.AddParameter(command, "friend_account_id", FriendAccountId);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public override string ToString()
        {
            return "AccountFriend(account_id: " + AccountId + ", friend_account_id: " + FriendAccountId + ", group_id: " + GroupId + ")";
        }
    }
}