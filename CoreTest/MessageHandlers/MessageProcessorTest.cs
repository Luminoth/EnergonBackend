using System;
using System.Threading.Tasks;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net.Sessions;
using EnergonSoftware.Core.Test.Messages;
using EnergonSoftware.Core.Test.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    [TestClass]
    public class MessageProcessorTest : IDisposable
    {
        private SessionManager _manager;
        private TestSession _session;

#region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(null != _session) {
                    _session.Dispose();
                }
            }
        }
#endregion

        [TestInitialize]
        public void SetUp()
        {
            _session = new TestSession();

            _manager = new SessionManager();
            _manager.Add(_session);
        }

        [TestCleanup]
        public void TearDown()
        {
            _manager.DisconnectAllAsync().Wait();
        }

        [TestMethod]
        public async Task TestQueueAsync()
        {
            _session.QueueMessage(new ExceptionMessage());
            await _session.RunAsync();
        }
    }
}
