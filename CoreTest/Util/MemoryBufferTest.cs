using System;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Core.Util;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.Test.Util
{
    [TestClass]
    public class MemoryBufferTest
    {
        private static readonly byte[] TestData = Encoding.UTF8.GetBytes("Test Data");

        private static async Task TestWriteAsync(MemoryBuffer buffer, byte[] data)
        {
            int length = buffer.Length;

            await buffer.WriteAsync(data, 0, data.Length).ConfigureAwait(false);
            Assert.AreEqual(length + data.Length, buffer.Position);
            Assert.AreEqual(length + data.Length, buffer.Length);
        }

        private static async Task TestReadAsync(MemoryBuffer buffer, byte[] expected)
        {
            byte[] data = new byte[buffer.Length];
            await buffer.ReadAsync(data, 0, data.Length).ConfigureAwait(false);
            CollectionAssert.AreEqual(expected, data);
        }

        private static void TestFlip(MemoryBuffer buffer)
        {
            int position = buffer.Position;

            buffer.Flip();
            Assert.AreEqual(position, buffer.Length);
            Assert.AreEqual(0, buffer.Position);
        }

        [TestMethod]
        public async Task TestReadAsync()
        {
            using(MemoryBuffer buffer = new MemoryBuffer()) {
                await TestWriteAsync(buffer, TestData).ConfigureAwait(false);
                TestFlip(buffer);
                await TestReadAsync(buffer, TestData).ConfigureAwait(false);
            }
        }

        [TestMethod]
        public async Task TestCompactAsync()
        {
            using(MemoryBuffer buffer = new MemoryBuffer()) {
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
            using(MemoryBuffer buffer = new MemoryBuffer()) {
                await TestWriteAsync(buffer, TestData).ConfigureAwait(false);

                buffer.Clear();
                Assert.AreEqual(0, buffer.Position);
                Assert.AreEqual(0, buffer.Length);
            }
        }

        [TestMethod]
        public async Task TestResetAsync()
        {
            using(MemoryBuffer buffer = new MemoryBuffer()) {
                await TestWriteAsync(buffer, TestData).ConfigureAwait(false);

                int length = buffer.Length;

                buffer.Reset();
                Assert.AreEqual(length, buffer.Position);
            }
        }
    }
}
