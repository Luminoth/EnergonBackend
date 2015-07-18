using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.UnitTests.IO
{
    [TestClass]
    public class MemoryStreamExtensionsTest
    {
        [TestMethod]
        public void Compact_State()
        {
            // Arraynge
            using(MemoryStream stream = new MemoryStream()) {

                // Act
                stream.WriteNetworkAsync("test").Wait();
                stream.WriteNetworkAsync("one").Wait();
                stream.WriteNetworkAsync("two").Wait();
                stream.Flip();

                string testString = stream.ReadNetworkStringAsync().Result;
                stream.CompactAsync().Wait();

                // Assert
                Assert.AreEqual("test", testString);
                Assert.AreEqual(stream.Length, stream.Position);
                // TODO: test something more concrete here to "prove"
                // that the buffer is actually compacted and not just flipped
            }
        }
    }
}
