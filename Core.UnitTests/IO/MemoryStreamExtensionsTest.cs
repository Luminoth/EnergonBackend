using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.UnitTests.IO
{
    [TestClass]
    public class MemoryStreamExtensionsTest
    {
        [TestMethod]
        public async Task CompactAsync_State()
        {
            // Arrange
            byte[][] testData = {
                Encoding.ASCII.GetBytes("test"),
                Encoding.ASCII.GetBytes("one"),
                Encoding.ASCII.GetBytes("twelve"),
            };

            using(MemoryStream stream = new MemoryStream()) {

                // Act
                await stream.WriteAsync(testData[0], 0, testData[0].Length).ConfigureAwait(false);
                await stream.WriteAsync(testData[1], 0, testData[1].Length).ConfigureAwait(false);
                await stream.WriteAsync(testData[2], 0, testData[2].Length).ConfigureAwait(false);
                stream.Flip();

                byte[] readData = new byte[testData[0].Length];
                await stream.ReadAsync(readData, 0, readData.Length).ConfigureAwait(false);
                await stream.CompactAsync().ConfigureAwait(false);

                // Assert
                CollectionAssert.AreEqual(testData[0], readData);
                Assert.AreEqual(stream.Length, stream.Position);
                Assert.AreEqual(testData[1].Length + testData[2].Length, stream.Length);
            }
        }
    }
}
