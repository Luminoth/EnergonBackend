using System.IO;
using System.Net;
using System.Threading.Tasks;

using EnergonSoftware.Backend.Accounts;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Backend.Test.Accounts
{
    [TestClass]
    public class AccountTest
    {
        private IMessageFormatter _formatter;

        [TestInitialize]
        public void SetUp()
        {
            _formatter = MessageFormatterFactory.Create(BinaryMessageFormatter.FormatterType);
        }

        [TestMethod]
        public async Task TestSerializationAsync()
        {
            Account account = new Account()
            {
                Id = 100,
                AccountName = "Test Account",
                SessionId = "123456",
                UserName = "Test User",
                EndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234),
                Visibility = Backend.Accounts.Visibility.Away,
                Status = "this is my status!",
            };

            Account deserializedAccount = new Account();
            using(MemoryStream buffer = new MemoryStream()) {
                _formatter.Attach(buffer);
                await account.SerializeAsync(_formatter).ConfigureAwait(false);
                await _formatter.FlushAsync().ConfigureAwait(false);

                buffer.Flip();

                await deserializedAccount.DeSerializeAsync(_formatter).ConfigureAwait(false);
            }

            Assert.AreEqual(account.UserName, deserializedAccount.UserName);
            Assert.AreEqual(account.Visibility, deserializedAccount.Visibility);
            Assert.AreEqual(account.Status, deserializedAccount.Status);
        }
    }
}
