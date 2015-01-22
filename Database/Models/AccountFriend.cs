﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

using EnergonSoftware.Core;

namespace EnergonSoftware.Database.Models
{
    public sealed class AccountFriend : IDatabaseObject
    {
        private static readonly TableDescription AccountFriendsTable = new TableDescription(
            "accounts_friends",
            new List<ColumnDescription>
            {
                { new ColumnDescription("account", DatabaseType.Integer).SetPrimaryKey().SetReferences("accounts", "id") },
                { new ColumnDescription("friend", DatabaseType.Integer).SetPrimaryKey().SetReferences("accounts", "id") },
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

        private long _account;
        public long Account { get { return _account; } set { _account = value; Dirty = true; } }

        private long _friend;
        public long Friend { get { return _friend; } set { _friend = value; Dirty = true; } }

        public AccountFriend()
        {
            _account = -1;
            _friend = -1;
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
        }

        public async Task InsertAsync(DatabaseConnection connection)
        {
            // account -> friend
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + AccountFriendsTable.Name + "(account, friend) VALUES(@account, @friend)")) {
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "friend", Friend);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // friend -> account
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + AccountFriendsTable.Name + "(account, friend) VALUES(@account, @friend)")) {
                connection.AddParameter(command, "account", Friend);
                connection.AddParameter(command, "friend", Account);
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
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

        public async Task UpdateAsync(DatabaseConnection connection)
        {
            await Task.Delay(0).ConfigureAwait(false);
            Clean();
        }

        public override string ToString()
        {
            return "AccountFriend(account: " + Account + ", friend: " + Friend + ")";
        }
    }
}