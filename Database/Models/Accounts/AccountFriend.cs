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
                { new ColumnDescription("account", DatabaseType.Integer).SetPrimaryKey().SetReferences("accounts", "id") },
                { new ColumnDescription("friend", DatabaseType.Integer).SetPrimaryKey().SetReferences("accounts", "id") },
                { new ColumnDescription("friend_group", DatabaseType.Integer).SetReferences("friend_groups", "id") },
            }
        );

        public static string TableName { get { return AccountFriendsTable.Name; } }

        public static async Task CreateTableAsync(DatabaseConnection connection)
        {
            await AccountFriendsTable.CreateAsync(connection).ConfigureAwait(false);
        }

        public static async Task DeleteAllAsync(DatabaseConnection connection, long accountId)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + AccountFriendsTable.Name + " WHERE account=@account OR friend=@friend")) {
                connection.AddParameter(command, "account", accountId);
                connection.AddParameter(command, "friend", accountId);
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

        private long _account = -1;
        public long Account { get { return _account; } set { _account = value; Dirty = true; } }

        private long _friend = -1;
        public long Friend { get { return _friend; } set { _friend = value; Dirty = true; } }

        private long _group = -1;
        public long Group { get { return _group; } set { _group = value; Dirty = true; } }

        public AccountFriend()
        {
        }

        public async Task<bool> ReadAsync(DatabaseConnection connection)
        {
            await Task.Delay(0).ConfigureAwait(false);
            return false;
        }

        public void Load(DbDataReader reader)
        {
            if(null == reader) {
                throw new ArgumentNullException("reader");
            }

            _account = reader.GetInt32(AccountFriendsTable["account"].Id);
            _friend = reader.GetInt32(AccountFriendsTable["friend"].Id);
            _group = reader.GetInt32(AccountFriendsTable["friend_group"].Id);
        }

        public async Task InsertAsync(DatabaseConnection connection)
        {
            // account -> friend
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + AccountFriendsTable.Name
                + "(account, friend, friend_group) VALUES(@account, @friend, @friend_group)"))
            {
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "friend", Friend);
                connection.AddParameter(command, "friend_group", Group);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // friend -> account
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + AccountFriendsTable.Name
                + "(account, friend, friend_group) VALUES(@account, @friend, @friend_group)"))
            {
                connection.AddParameter(command, "account", Friend);
                connection.AddParameter(command, "friend", Account);
                connection.AddParameter(command, "friend_group", Group);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public async Task UpdateAsync(DatabaseConnection connection)
        {
            await Task.Delay(0).ConfigureAwait(false);
            Clean();
        }

        public async Task DeleteAsync(DatabaseConnection connection)
        {
            // account -> friend
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + AccountFriendsTable.Name + " WHERE account=@account AND friend=@friend")) {
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "friend", Friend);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // friend -> account
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + AccountFriendsTable.Name + " WHERE account=@account AND friend=@friend")) {
                connection.AddParameter(command, "account", Friend);
                connection.AddParameter(command, "friend", Account);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }

        public override string ToString()
        {
            return "AccountFriend(account: " + Account + ", friend: " + Friend + ", group: " + Group + ")";
        }
    }
}