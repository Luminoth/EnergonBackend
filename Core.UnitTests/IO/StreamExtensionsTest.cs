using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.UnitTests.IO
{
    [TestClass]
    public class StreamExtensionsTest
    {
        private static readonly byte[] TestData = Encoding.UTF8.GetBytes("Test Data");

        private static async Task TestWriteAsync(Stream stream, byte[] data)
        {
            long length = stream.Length;

            await stream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            Assert.AreEqual(length + data.Length, stream.Position);
            Assert.AreEqual(length + data.Length, stream.Length);
        }

        private static async Task TestReadAsync(Stream stream, byte[] expected)
        {
            byte[] data = new byte[stream.Length];
            await stream.ReadAsync(data, 0, data.Length).ConfigureAwait(false);
            CollectionAssert.AreEqual(expected, data);
        }

        private static void TestFlip(Stream stream)
        {
            long position = stream.Position;

            stream.Flip();
            Assert.AreEqual(position, stream.Length);
            Assert.AreEqual(0, stream.Position);
        }

        [TestMethod]
        public async Task TestReadAsync()
        {
            using(MemoryStream buffer = new MemoryStream()) {
                await TestWriteAsync(buffer, TestData).ConfigureAwait(false);
                TestFlip(buffer);
                await TestReadAsync(buffer, TestData).ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task TestCompactAsync()
        {
            using(MemoryStream buffer = new MemoryStream()) {
                await TestWriteAsync(buffer, TestData).ConfigureAwait(false);
                await TestWriteAsync(buffer, TestData).ConfigureAwait(false);
                TestFlip(buffer);
                await TestReadAsync(buffer, TestData).ConfigureAwait(false);

// TODO: test compact!
            }
        }

        [TestMethod]
        public async Task TestWriteClearAsync()
        {
            using(MemoryStream buffer = new MemoryStream()) {
                await TestWriteAsync(buffer, TestData).ConfigureAwait(false);

                buffer.Clear();
                Assert.AreEqual(0, buffer.Position);
                Assert.AreEqual(0, buffer.Length);
            }
        }

        [TestMethod]
        public async Task TestResetAsync()
        {
            using(MemoryStream buffer = new MemoryStream()) {
                await TestWriteAsync(buffer, TestData).ConfigureAwait(false);

                long length = buffer.Length;

                buffer.Reset();
                Assert.AreEqual(length, buffer.Position);
            }
        }
    }
}
