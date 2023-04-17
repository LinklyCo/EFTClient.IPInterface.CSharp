using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PCEFTPOS.EFTClient.IPInterface;
using PCEFTPOS.EFTClient.IPInterface.Slave;
using Xunit;

namespace IPInterface.Test
{
    public class EFTClientIPAsyncTest
    {
        private const string TestHostName = "TestHost";
        private const int TestHostPort = 1;
        
        #region ConnectAsync

        [Fact]
        public void ConnectAsync_CallsSocketConnectAsyncWithCorrectArgs_ReturnsTrue()
        {
            bool useSSL = true;
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();

            var actual = client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort, useSSL).Result;

            mockTcpSocket.Verify(s => s.ConnectAsync(TestHostName, TestHostPort, useSSL), Times.Once);
            mockTcpSocket.VerifyAdd(s => s.OnLog += It.IsAny<EventHandler<LogEventArgs>>(), Times.Once);

            Assert.Equal(client.LogLevel, mockTcpSocket.Object.LogLevel);
            Assert.True(actual);
        }

        [Fact]
        public void ConnectAsync_CallingAgainDisconnectsAndReplacesSocket_ReturnsTrue()
        {
            bool useSSL = true;
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var secondMockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();

            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort, useSSL).Wait();
            var actual = client.ConnectAsync(b => secondMockTcpSocket.Object, TestHostName, TestHostPort, useSSL).Result;

            // verify first socket connects once and is closed once, also that Dispose() is called on it since it is replaced with the second socket
            mockTcpSocket.Verify(s => s.ConnectAsync(TestHostName, TestHostPort, useSSL), Times.Once);
            mockTcpSocket.Verify(s => s.Close(), Times.Once);
            mockTcpSocket.VerifyRemove(s => s.OnLog -= It.IsAny<EventHandler<LogEventArgs>>(), Times.Once);
            mockTcpSocket.Verify(s => s.Dispose(), Times.Once);


            // verify connect called on second socket
            secondMockTcpSocket.Verify(s => s.ConnectAsync(TestHostName, TestHostPort, useSSL), Times.Once);
            secondMockTcpSocket.VerifyAdd(s => s.OnLog += It.IsAny<EventHandler<LogEventArgs>>(), Times.Once);

            Assert.Equal(client.LogLevel, mockTcpSocket.Object.LogLevel);
            Assert.True(actual);
        }

        #endregion

        #region Disconnect
        [Fact]
        public void Disconnect_CallsStop_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();

            client.Disconnect();

            mockTcpSocket.Verify(s => s.Close(), Times.Once);
            mockTcpSocket.VerifyRemove(s => s.OnLog -= It.IsAny<EventHandler<LogEventArgs>>(), Times.Once);
        }

        #endregion

        #region WriteRequestAsync

        private static readonly EFTLogonRequest EftLogonRequest = new EFTLogonRequest();

        [Fact]
        public void WriteRequestAsync_Connected_CallsSocketWriteRequestAsync_ReturnsTrue()
        {
            var request = new EFTLogonRequest();
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();
            var actual = client.WriteRequestAsync(request).Result;

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.WriteRequestAsync(It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [ClassData(typeof(EftRequestsAndGeneratedStrings))]
        public void WriteRequestAsync_Connected_SendsCorrectString_ReturnsTrue(EFTRequest request, string expectedSent)
        {
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();

            var actual = client.WriteRequestAsync(request).Result;

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.WriteRequestAsync(expectedSent), Times.Once);
        }

        [Fact]
        public async Task WriteRequestAsync_ConnectNeverCalled_ThrowsIvalidOperationException()
        {
            var request = new EFTLogonRequest();
            var client = new EFTClientIPAsyncWrapper();
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.WriteRequestAsync(request));
        }

        /// <summary>
        /// Validates that if the socket raises an exception (that isn't a ConnectionException)
        /// during a EFTClientIPAsync.WriteRequestAsync call 
        /// </summary>
        [Fact]
        public async Task WriteRequestAsync_SocketException_ThrowsConnectionException()
        {
            var request = new EFTLogonRequest();
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();

            mockTcpSocket.Setup(s => s.WriteRequestAsync(It.IsAny<string>())).Throws(() => new ArgumentNullException());
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();
            client.Disconnect();
            await Assert.ThrowsAsync<ConnectionException>(() => client.WriteRequestAsync(request));
        }

        [Fact]
        public async Task WriteRequestAsync_NullInput_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();
            client.Disconnect();
            await Assert.ThrowsAsync<NullReferenceException>(() => client.WriteRequestAsync(null));
        }

        #endregion

        #region Dispose

        [Fact]
        public void Dispose_CallsDisposeOnSocket()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();
            client.Dispose();

            mockTcpSocket.Verify(s => s.Dispose(), Times.Once);
        }

        #endregion

        #region CheckConnectStateAsync

        [Fact]
        public void CheckConnectStateAsync_Connected_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();

            Assert.True( client.CheckConnectStateAsync().Result );
        }


        [Fact]
        public void CheckConnectStateAsync_Disconnected_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockDisconnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();

            // we have to call connect so that we don't get a null reference exception
            // the mock ensures that the socket considers itself disconnected
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();

            Assert.False(client.CheckConnectStateAsync().Result);
        }

        [Fact]
        public void IsConnected_NeverConnected_ReturnsFalse()
        {
            var client = new EFTClientIPAsync();

            Assert.False(client.CheckConnectStateAsync().Result);
        }

        #endregion

        #region IsConnected

        [Fact]
        public void IsConnected_Connected_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();

            Assert.True(client.IsConnected);
        }


        [Fact]
        public void IsConnected_Disconnected_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockDisconnectedTcpSocketAsync();
            var client = new EFTClientIPAsyncWrapper();

            // we have to call connect so that we don't get a null reference exception
            // the mock ensures that the socket considers itself disconnected
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();

            Assert.False(client.IsConnected);
        }

        [Fact]
        public void CheckConnectStateAsync_NeverConnected_ReturnsFalse()
        {
            var client = new EFTClientIPAsync();

            Assert.False(client.IsConnected);
        }

        #endregion

        #region ReadResponseAsync

        [Fact]
        public void ReadResponseAsync_GenuineResponse_ReturnsCorrectResponseType()
        {
            // todo make theory and do this for more response types? Would have to create valid response strings somewhere
            var mockTcpSocket = CreateMockConnectedTcpSocketAsyncWithReadResponse("#0009K00000");
            var client = new EFTClientIPAsyncWrapper();
            client.ConnectAsync(b => mockTcpSocket.Object, TestHostName, TestHostPort).Wait();

            var response = client.ReadResponseAsync<EFTStatusResponse>(CancellationToken.None).Result;

            Assert.Equal(typeof(EFTStatusResponse), response.GetType());
            mockTcpSocket.Verify(s => s.ReadResponseAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion



        #region Testing Utils

        private Mock<ITcpSocketAsync> CreateMockConnectedTcpSocketAsyncWithReadResponse(string response)
        {
            var mock = CreateMockConnectedTcpSocketAsync();
            mock.Setup(s => s.ReadResponseAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .Returns(async (byte[] buf, CancellationToken t) => await Task.Run(() => DirectEncoding.DIRECT.GetBytes(response, 0, response.Length, buf, 0), t));

            return mock;
        }


        private Mock<ITcpSocketAsync> CreateMockConnectedTcpSocketAsync()
        {
            var mock = new Mock<ITcpSocketAsync>();

            // Considers itself connected
            mock.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(Task.FromResult(true));
            mock.Setup(s => s.IsConnected).Returns(true);
            mock.Setup(s => s.CheckConnectStateAsync()).Returns(Task.FromResult(true));

            // make .WriteRequestAsync return true  to mock successful sending
            mock.Setup(s => s.WriteRequestAsync(It.IsAny<string>())).Returns(Task.FromResult(true));

            mock.Setup(s => s.LogLevel);

            return mock;
        }

        private Mock<ITcpSocketAsync> CreateMockDisconnectedTcpSocketAsync()
        {
            var mock = new Mock<ITcpSocketAsync>();

            // Considers itself disconnected
            mock.Setup(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(Task.FromResult(false));
            mock.Setup(s => s.IsConnected).Returns(false);
            mock.Setup(s => s.CheckConnectStateAsync()).Returns(Task.FromResult(false));

            mock.Setup(s => s.WriteRequestAsync(It.IsAny<string>())).Returns(Task.FromResult(false));

            mock.Setup(s => s.LogLevel);

            return mock;
        }

        /// <summary>
        /// Simple wrapper to allow us to call the protected connect async function we want that
        /// lets us mock the ITcpSocketAsync use by the EFTClientIPAsyncWrapper
        /// </summary>

        internal class EFTClientIPAsyncWrapper : EFTClientIPAsync
        {
            /// <summary>
            /// Calls the protected ConnectAsync function,
            /// We use new to make this public rather than protected which is a little bit of a hack
            /// </summary>
            public new Task<bool> ConnectAsync(Func<bool, ITcpSocketAsync> socketFactory, string hostName, int hostPort, bool useSSL=false, bool useKeepAlive=false)
            {
                return base.ConnectAsync(socketFactory, hostName, hostPort, useSSL, useKeepAlive);
            }
        }

        #endregion
    } // EFTClientIPAsyncTest

} // namespace
