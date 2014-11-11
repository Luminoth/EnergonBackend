using System.Collections.Generic;
using System.Data.Common;

using EnergonSoftware.Core;

namespace EnergonSoftware.Database.Objects
{
    public sealed class AccountFriend : IDatabaseObject
    {
        private static readonly TableDescription ACCOUNT_FRIENDS_TABLE = new TableDescription("accounts_friends",
            new List<ColumnDescription>
            {
                { new ColumnDescription("account", DatabaseType.Integer).SetPrimaryKey().SetReferences("accounts", "id") },
                { new ColumnDescription("friend", DatabaseType.Integer).SetPrimaryKey().SetReferences("accounts", "id") },
            }
        );

        public static string TableName { get { return ACCOUNT_FRIENDS_TABLE.Name; } }

        public static void CreateTable(DatabaseConnection connection)
        {
            ACCOUNT_FRIENDS_TABLE.Create(connection);
        }

        public static void DeleteAll(DatabaseConnection connection, long accountId)
        {
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + ACCOUNT_FRIENDS_TABLE.Name + " WHERE account=@account OR friend=@friend")) {
                connection.AddParameter(command, "account", accountId);
                connection.AddParameter(command, "friend", accountId);
                command.ExecuteNonQuery();
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

        public AccountFriend(long account, long friend)
        {
            _account = account;
            _friend = friend;
        }

        public bool Read(DatabaseConnection connection)
        {
            return false;
        }

        public void Load(DbDataReader reader)
        {
            _account = reader.GetInt32(ACCOUNT_FRIENDS_TABLE["account"].Id);
            _friend = reader.GetInt32(ACCOUNT_FRIENDS_TABLE["friend"].Id);
        }

        public void Insert(DatabaseConnection connection)
        {
            // account -> friend
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + ACCOUNT_FRIENDS_TABLE.Name + "(account, friend) VALUES(@account, @friend)")) {
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "friend", Friend);
                command.ExecuteNonQuery();
            }

            // friend -> account
            using(DbCommand command = connection.BuildCommand("INSERT INTO " + ACCOUNT_FRIENDS_TABLE.Name + "(account, friend) VALUES(@account, @friend)")) {
                connection.AddParameter(command, "account", Friend);
                connection.AddParameter(command, "friend", Account);
                command.ExecuteNonQuery();
            }
        }

        public void Delete(DatabaseConnection connection)
        {
            // account -> friend
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + ACCOUNT_FRIENDS_TABLE.Name + " WHERE account=@account AND friend=@friend")) {
                connection.AddParameter(command, "account", Account);
                connection.AddParameter(command, "friend", Friend);
                command.ExecuteNonQuery();
            }

            // friend -> account
            using(DbCommand command = connection.BuildCommand("DELETE FROM " + ACCOUNT_FRIENDS_TABLE.Name + " WHERE account=@account AND friend=@friend")) {
                connection.AddParameter(command, "account", Friend);
                connection.AddParameter(command, "friend", Account);
                command.ExecuteNonQuery();
            }
        }

        public void Update(DatabaseConnection connection)
        {
            Clean();
        }

        public override string ToString()
        {
            return "AccountFriend(account: " + Account + ", friend: " + Friend + ")";
        }
    }
}