using System;
using System.Linq;
using System.Threading.Tasks;

using EnergonSoftware.DAL;
using EnergonSoftware.DAL.Models.Accounts;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.DAL.Test.Models.Accounts
{
    [TestClass]
    public class AccountInfoTest
    {
        [TestMethod]
        public async Task TestAccountInfoAsync()
        {
            using(AccountsDatabaseContext context = new AccountsDatabaseContext()) {
                AccountInfo write = new AccountInfo()
                {
                    AccountName = "unittest1",
                    EmailAddress = "unittest1@energonsoftware.org",
                    UserName = "Unit Test 1",
                    PasswordMD5 = "1234",
                    PasswordSHA512 = "1234",
                };
                context.Accounts.Add(write);
                await context.SaveChangesAsync().ConfigureAwait(false);

                AccountInfo read = await context.GetAccountAsync(write.AccountName).ConfigureAwait(false);
                Assert.IsNotNull(read);

                Assert.AreEqual(write.Id, read.Id);
                Assert.AreEqual(write.AccountName, read.AccountName);
                Assert.AreEqual(write.EmailAddress, read.EmailAddress);
                Assert.AreEqual(write.UserName, read.UserName);
                Assert.AreEqual(write.PasswordMD5, read.PasswordMD5);
                Assert.AreEqual(write.PasswordSHA512, read.PasswordSHA512);

                context.Accounts.Remove(read);
                await context.SaveChangesAsync().ConfigureAwait(false);

                read = await context.GetAccountAsync(write.AccountName).ConfigureAwait(false);
                Assert.IsNull(read);
            }
        }
    }
}
