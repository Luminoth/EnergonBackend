using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Net;
using EnergonSoftware.Core.Test.Net;
using EnergonSoftware.Core.Test.Messages;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    [TestClass]
    public class MessageProcessorTest
    {
        private SessionManager _manager;
        private TestSession _session;

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
