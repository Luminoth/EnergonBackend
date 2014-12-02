using System;
using System.Text;

using EnergonSoftware.Core.Util;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.Test.Util
{
    [TestClass]
    public class MemoryBufferTest
    {
        private static readonly byte[] TestData = Encoding.UTF8.GetBytes("Test Data");

        private static void TestWrite(MemoryBuffer buffer, byte[] data)
        {
            int length = buffer.Length;

            buffer.Write(data, 0, data.Length);
            Assert.AreEqual(length + data.Length, buffer.Position);
            Assert.AreEqual(length + data.Length, buffer.Length);
        }

        private static void TestRead(MemoryBuffer buffer, byte[] expected)
        {
            byte[] data = new byte[buffer.Length];
            buffer.Read(data, 0, data.Length);
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
        public void TestRead()
        {
            using(MemoryBuffer buffer = new MemoryBuffer()) {
                TestWrite(buffer, TestData);
                TestFlip(buffer);
                TestRead(buffer, TestData);
            }
        }

        [TestMethod]
        public void TestCompact()
        {
            using(MemoryBuffer buffer = new MemoryBuffer()) {
                TestWrite(buffer, TestData);
                TestWrite(buffer, TestData);
                TestFlip(buffer);
                TestRead(buffer, TestData);

// TODO: test compact!
            }
        }

        [TestMethod]
        public void TestWriteClear()
        {
            using(MemoryBuffer buffer = new MemoryBuffer()) {
                TestWrite(buffer, TestData);

                buffer.Clear();
                Assert.AreEqual(0, buffer.Position);
                Assert.AreEqual(0, buffer.Length);
            }
        }

        [TestMethod]
        public void TestReset()
        {
            using(MemoryBuffer buffer = new MemoryBuffer()) {
                TestWrite(buffer, TestData);

                int length = buffer.Length;

                buffer.Reset();
                Assert.AreEqual(length, buffer.Position);
            }
        }
    }
}
