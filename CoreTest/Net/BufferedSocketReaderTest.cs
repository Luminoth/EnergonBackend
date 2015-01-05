using System;
using System.Net.Sockets;
using System.Net.Sockets.Fakes;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Core.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.Test.Net
{
    [TestClass]
    public class BufferedSocketReaderTest
    {
        private static readonly byte[] TestData = Encoding.UTF8.GetBytes("Test Data");

        private static async Task TestPollAndReadAsync(BufferedSocketReader reader)
        {
            // TODO: find a way to force the stub to give us some data

            int count = await reader.PollAndReadAsync().ConfigureAwait(false);
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task TestPollAndReadTcp4Async()
        {
            using(StubSocket socket = new StubSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                using(BufferedSocketReader reader = new BufferedSocketReader(socket)) {
                    await TestPollAndReadAsync(reader).ConfigureAwait(false);
                }
            }
        }

        [TestMethod]
        public async Task TestPollAndReadTcp6Async()
        {
            using(StubSocket socket = new StubSocket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)) {
                using(BufferedSocketReader reader = new BufferedSocketReader(socket)) {
                    await TestPollAndReadAsync(reader).ConfigureAwait(false);
                }
            }
        }

        [TestMethod]
        public async Task TestPollAndReadUdp4Async()
        {
            using(StubSocket socket = new StubSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)) {
                using(BufferedSocketReader reader = new BufferedSocketReader(socket)) {
                    await TestPollAndReadAsync(reader).ConfigureAwait(false);
                }
            }
        }

        [TestMethod]
        public async Task TestPollAndReadUdp6Async()
        {
            using(StubSocket socket = new StubSocket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp)) {
                using(BufferedSocketReader reader = new BufferedSocketReader(socket)) {
                    await TestPollAndReadAsync(reader).ConfigureAwait(false);
                }
            }
        }
    }
}
