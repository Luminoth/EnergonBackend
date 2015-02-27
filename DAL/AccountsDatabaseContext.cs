using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

using EnergonSoftware.DAL.Models.Accounts;

using log4net;

namespace EnergonSoftware.DAL
{
    public class AccountsDatabaseContext : DbContext
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(AccountsDatabaseContext));

        public virtual DbSet<AccountInfo> Accounts { get; set; }

        public AccountsDatabaseContext() : base("name=EnergonSoftwareAccounts")
        {
            Database.Log = DebugLog;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try {
                await Database.Connection.OpenAsync();
                return true;
            } catch(Exception e) {
                Logger.Warn(e.ToString());
                return false;
            }
        }

        public async Task<AccountInfo> GetAccountAsync(string accountName)
        {
            var accounts = from a in Accounts where a.AccountName == accountName select a;
            if(accounts.Count() < 1) {
                return null;
            }

            return await accounts.FirstAsync().ConfigureAwait(false);
        }

        private void DebugLog(string sql)
        {
            Logger.Debug(sql);
        }
    }
}
