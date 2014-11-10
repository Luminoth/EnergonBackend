using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using EnergonSoftware.Core.MessageHandlers;
using EnergonSoftware.Core.Test.Net;
using EnergonSoftware.Core.Test.Messages;

namespace EnergonSoftware.Core.Test.MessageHandlers
{
    [TestClass]
    public class MessageProcessorTest
    {
        private MessageProcessor _processor;
        private TestSession _session;

        [TestInitialize]
        public void Initialize()
        {
            _session = new TestSession();

            _processor = new MessageProcessor();
            _processor.Start(new MessageHandlerFactory());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _processor.Stop();
        }

        [TestMethod]
        public void TestQueue()
        {
            _processor.QueueMessage(_session, new ExceptionMessage());
// TODO: so what are we looking for here?
        }
    }
}
