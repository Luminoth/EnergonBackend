using System.Net;

using EnergonSoftware.Core.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnergonSoftware.Core.UnitTests.Net
{
    [TestClass]
    public class NetUtilTests
    {
// TODO: test socket connectors

#region CompareEndPoints Tests
        [TestMethod]
        public void CompareEndPoints_IPAddress_Same()
        {
            // Arrange
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            EndPoint endPoint = new IPEndPoint(ipAddress, 1234);

            // Act
            bool sameEndPoint = NetUtil.CompareEndPoints(endPoint.ToString(), endPoint);

            // Assert
            Assert.IsTrue(sameEndPoint);
        }

        [TestMethod]
        public void CompareEndPoints_IPAddress_Equivalent()
        {
            // Arrange
            IPAddress ipAddress1 = IPAddress.Parse("127.0.0.1");
            IPAddress ipAddress2 = IPAddress.Parse("127.0.0.1");

            EndPoint endPoint1 = new IPEndPoint(ipAddress1, 1234);
            EndPoint endPoint2 = new IPEndPoint(ipAddress2, 1234);

            // Act
            bool sameEndPoint1 = NetUtil.CompareEndPoints(endPoint1.ToString(), endPoint2);
            bool sameEndPoint2 = NetUtil.CompareEndPoints(endPoint2.ToString(), endPoint1);

            // Assert
            Assert.IsTrue(sameEndPoint1);
            Assert.IsTrue(sameEndPoint2);
        }

        [TestMethod]
        public void CompareEndPoints_IPAddress_DifferentPort()
        {
            // Arrange
            IPAddress ipAddress1 = IPAddress.Parse("127.0.0.1");
            IPAddress ipAddress2 = IPAddress.Parse("127.0.0.1");

            EndPoint endPoint1 = new IPEndPoint(ipAddress1, 1234);
            EndPoint endPoint2 = new IPEndPoint(ipAddress2, 2345);

            // Act
            bool sameEndPoint1 = NetUtil.CompareEndPoints(endPoint1.ToString(), endPoint2);
            bool sameEndPoint2 = NetUtil.CompareEndPoints(endPoint2.ToString(), endPoint1);

            // Assert
            Assert.IsTrue(sameEndPoint1);
            Assert.IsTrue(sameEndPoint2);
        }

        [TestMethod]
        public void CompareEndPoints_IPAddress_DifferentAddress()
        {
            // Arrange
            IPAddress ipAddress1 = IPAddress.Parse("127.0.0.1");
            IPAddress ipAddress2 = IPAddress.Parse("10.0.0.1");

            EndPoint endPoint1 = new IPEndPoint(ipAddress1, 1234);
            EndPoint endPoint2 = new IPEndPoint(ipAddress2, 1234);

            // Act
            bool sameEndPoint1 = NetUtil.CompareEndPoints(endPoint1.ToString(), endPoint2);
            bool sameEndPoint2 = NetUtil.CompareEndPoints(endPoint2.ToString(), endPoint1);

            // Assert
            Assert.IsFalse(sameEndPoint1);
            Assert.IsFalse(sameEndPoint2);
        }

        [TestMethod]
        public void CompareEndPoints_IPAddress_Different()
        {
            // Arrange
            IPAddress ipAddress1 = IPAddress.Parse("127.0.0.1");
            IPAddress ipAddress2 = IPAddress.Parse("10.0.0.1");

            EndPoint endPoint1 = new IPEndPoint(ipAddress1, 1234);
            EndPoint endPoint2 = new IPEndPoint(ipAddress2, 2345);

            // Act
            bool sameEndPoint1 = NetUtil.CompareEndPoints(endPoint1.ToString(), endPoint2);
            bool sameEndPoint2 = NetUtil.CompareEndPoints(endPoint2.ToString(), endPoint1);

            // Assert
            Assert.IsFalse(sameEndPoint1);
            Assert.IsFalse(sameEndPoint2);
        }
    }
#endregion
}
