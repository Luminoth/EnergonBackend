﻿using System.Text;

using EnergonSoftware.Core.Net.Sessions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.Test.Net.Sessions
{
    [TestClass]
    public class NetworkSessionTest
    {
        private NetworkSessionManager _sessionManager;

        [TestInitialize]
        public void SetUp()
        {
            _sessionManager = new NetworkSessionManager();
        }

        [TestCleanup]
        public void TearDown()
        {
            _sessionManager.DisconnectAllAsync().Wait();
        }

        [TestMethod]
        public void TestAddSession()
        {
            SetupManager(6);
        }

        [TestMethod]
        public void TestDisconnectAll()
        {
            SetupManager(10);
            _sessionManager.DisconnectAllAsync().Wait();
            Assert.IsTrue(_sessionManager.IsEmpty);
        }

        [TestMethod]
        public void TestProcessData()
        {
            TestSession session = new TestSession();

            Assert.IsFalse(session.MessageProcessed);
            session.ProcessData(Encoding.UTF8.GetBytes("test"));
            Assert.IsTrue(session.MessageProcessed);
        }

        private void SetupManager(int count)
        {
            for(int i=0; i<count; ++i) {
                TestSession session = new TestSession();
                _sessionManager.Add(session);
            }
            Assert.IsFalse(_sessionManager.IsEmpty);
            Assert.AreEqual(count, _sessionManager.Count);
        }
    }
}
