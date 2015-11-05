using System.IO;
using System.Text;
using System.Threading.Tasks;

using EnergonSoftware.Core.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.UnitTests.IO
{
    [TestClass]
    public class StreamExtensionsTest
    {
#region IndexOf Tests
        [TestMethod]
        public async Task IndexOfAsync_Start_Exists()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");
            byte[] searchFor = Encoding.ASCII.GetBytes("this");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long index = await stream.IndexOfAsync(searchFor).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(0, index);
                Assert.AreEqual(0, stream.Position);
            }
        }

        [TestMethod]
        public async Task IndexOfAsync_Middle_Exists()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");
            byte[] searchFor = Encoding.ASCII.GetBytes("some");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long index = await stream.IndexOfAsync(searchFor).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(8, index);
                Assert.AreEqual(0, stream.Position);
            }
        }

        [TestMethod]
        public async Task IndexOfAsync_End_Exists()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");
            byte[] searchFor = Encoding.ASCII.GetBytes("data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long index = await stream.IndexOfAsync(searchFor).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(18, index);
                Assert.AreEqual(0, stream.Position);
            }
        }

        [TestMethod]
        public async Task IndexOfAsync_NotExists()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");
            byte[] searchFor = Encoding.ASCII.GetBytes("fail");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long index = await stream.IndexOfAsync(searchFor).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(-1, index);
                Assert.AreEqual(0, stream.Position);
            }
        }
#endregion

#region Consume Tests
        [TestMethod]
        public async Task ConsumeAsync_Zero()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long read = await stream.ConsumeAsync(0).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(read, 0);
                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(testData.Length, stream.Length);
            }
        }

        [TestMethod]
        public async Task ConsumeAsync_Negative()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long read = await stream.ConsumeAsync(-1).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(read, 0);
                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(testData.Length, stream.Length);
            }
        }

        [TestMethod]
        public async Task ConsumeAsync_LessThanLength()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long read = await stream.ConsumeAsync(5).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(read, 5);
                Assert.AreEqual(5, stream.Position);
                Assert.AreEqual(testData.Length, stream.Length);
            }
        }

        [TestMethod]
        public async Task ConsumeAsync_ExactlyLength()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long read = await stream.ConsumeAsync(testData.Length).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(read, testData.Length);
                Assert.AreEqual(testData.Length, stream.Position);
                Assert.AreEqual(testData.Length, stream.Length);
            }
        }

        [TestMethod]
        public async Task ConsumeAsync_GreaterThanLength()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                long read = await stream.ConsumeAsync(testData.Length + 100).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(read, testData.Length);
                Assert.AreEqual(testData.Length, stream.Position);
                Assert.AreEqual(testData.Length, stream.Length);
            }
        }
#endregion

#region Read/Write Tests
        [TestMethod]
        public async Task ReadWriteByteAsync()
        {
            // Arrange
            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteByteAsync(123).ConfigureAwait(false);
                long position = stream.Position;
                long length = stream.Length;

                stream.Flip();

                int b = await stream.ReadByteAsync().ConfigureAwait(false);

                // Assert
                Assert.AreEqual(1, position);
                Assert.AreEqual(1, length);
                Assert.AreEqual(123, b);
            }
        }

        [TestMethod]
        public async Task ReadWriteAsync_Byte()
        {
            // Arrange
            byte write = 123;
            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(write).ConfigureAwait(false);
                long position = stream.Position;
                long length = stream.Length;

                stream.Flip();

                int read = await stream.ReadByteAsync().ConfigureAwait(false);

                // Assert
                Assert.AreEqual(1, position);
                Assert.AreEqual(1, length);
                Assert.AreEqual(123, read);
            }
        }

        [TestMethod]
        public async Task ReadWriteAsync_Bool_True()
        {
            // Arrange
            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(true).ConfigureAwait(false);
                long position = stream.Position;
                long length = stream.Length;

                stream.Flip();

                bool read = await stream.ReadBoolAsync().ConfigureAwait(false);

                // Assert
                Assert.AreEqual(1, position);
                Assert.AreEqual(1, length);
                Assert.AreEqual(true, read);
            }
        }

        [TestMethod]
        public async Task ReadWriteAsync_Bool_False()
        {
            // Arrange
            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(false).ConfigureAwait(false);
                long position = stream.Position;
                long length = stream.Length;

                stream.Flip();

                bool read = await stream.ReadBoolAsync().ConfigureAwait(false);

                // Assert
                Assert.AreEqual(1, position);
                Assert.AreEqual(1, length);
                Assert.AreEqual(false, read);
            }
        }
#endregion

#region Peek Tests
        [TestMethod]
        public async Task PeekAsync()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");
            byte[] expectedPeekData = Encoding.ASCII.GetBytes("this is som");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                byte[] peekData = new byte[expectedPeekData.Length];
                int read = await stream.PeekAsync(peekData, 0, peekData.Length).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(expectedPeekData.Length, read);
                CollectionAssert.AreEqual(expectedPeekData, peekData);
            }
        }
#endregion

// TODO: test network read/write/peek methods

#region Buffer Tests
        [TestMethod]
        public async Task GetHasRemaining_NoRead()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                // Assert
                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(testData.Length, stream.Length);
                Assert.IsTrue(stream.HasRemaining());
                Assert.AreEqual(testData.Length, stream.GetRemaining());
            }
        }

        [TestMethod]
        public async Task GetHasRemaining_SomeRemaining()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                byte[] readData = new byte[5];
                await stream.ReadAsync(readData, 0, readData.Length).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(readData.Length, stream.Position);
                Assert.AreEqual(testData.Length, stream.Length);
                Assert.IsTrue(stream.HasRemaining());
                Assert.AreEqual(testData.Length - readData.Length, stream.GetRemaining());
            }
        }

        [TestMethod]
        public async Task GetHasRemaining_NoneRemaining()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();

                byte[] readData = new byte[testData.Length];
                await stream.ReadAsync(readData, 0, readData.Length).ConfigureAwait(false);

                // Assert
                Assert.AreEqual(readData.Length, stream.Position);
                Assert.AreEqual(testData.Length, stream.Length);
                Assert.IsFalse(stream.HasRemaining());
                Assert.AreEqual(testData.Length - readData.Length, stream.GetRemaining());
            }
        }

        [TestMethod]
        public async Task Clear()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Clear();

                // Assert
                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(0, stream.Length);
                Assert.IsFalse(stream.HasRemaining());
            }
        }

        [TestMethod]
        public async Task Flip()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                long position = stream.Position;

                stream.Flip();

                // Assert
                Assert.AreEqual(0, stream.Position);
                Assert.AreEqual(position, stream.Length);
                Assert.IsTrue(stream.HasRemaining());
            }
        }

        [TestMethod]
        public async Task Reset()
        {
            // Arrange
            byte[] testData = Encoding.ASCII.GetBytes("this is some test data");

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData, 0, testData.Length).ConfigureAwait(false);
                stream.Flip();
                stream.Reset();

                // Assert
                Assert.AreEqual(stream.Position, stream.Length);
                Assert.AreNotEqual(0, stream.Position);
            }
        }
    }
#endregion
}
