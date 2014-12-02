using System;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
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
                    _session = null;
                }
            }
        }
#endregion

        [TestInitialize]
        public void Initialize()
        {
            _manager = new SessionManager();
            _manager.Start(new MessageHandlerFactory());

            _session = new TestSession(_manager);
            _manager.Add(_session);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _manager.Stop();
        }

        [TestMethod]
        public void TestQueue()
        {
// TODO: so what are we looking for here?
            //_session.QueueMessage(_session, new ExceptionMessage());
        }
    }
}
