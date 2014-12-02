using System;
using System.Net.Sockets;
using System.Net.Sockets.Fakes;
using System.Text;

using EnergonSoftware.Core.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.Test.Net
{
    [TestClass]
    public class BufferedSocketReaderTest
    {
        private static readonly byte[] TestData = Encoding.UTF8.GetBytes("Test Data");

        private static void TestPollAndRead(BufferedSocketReader reader)
        {
            // TODO: find a way to force the stub to give us some data

            int count = reader.PollAndRead();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public void TestPollAndReadTcp4()
        {
            using(StubSocket socket = new StubSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                using(BufferedSocketReader reader = new BufferedSocketReader(socket)) {
                    TestPollAndRead(reader);
                }
            }
        }

        [TestMethod]
        public void TestPollAndReadTcp6()
        {
            using(StubSocket socket = new StubSocket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)) {
                using(BufferedSocketReader reader = new BufferedSocketReader(socket)) {
                    TestPollAndRead(reader);
                }
            }
        }

        [TestMethod]
        public void TestPollAndReadUdp4()
        {
            using(StubSocket socket = new StubSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                using(BufferedSocketReader reader = new BufferedSocketReader(socket)) {
                    TestPollAndRead(reader);
                }
            }
        }

        [TestMethod]
        public void TestPollAndReadUdp6()
        {
            using(StubSocket socket = new StubSocket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)) {
                using(BufferedSocketReader reader = new BufferedSocketReader(socket)) {
                    TestPollAndRead(reader);
                }
            }
        }
    }
}
