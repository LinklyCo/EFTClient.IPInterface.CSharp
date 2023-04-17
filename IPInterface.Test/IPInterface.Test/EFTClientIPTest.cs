using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Moq;
using PCEFTPOS.EFTClient.IPInterface;
using PCEFTPOS.EFTClient.IPInterface.Slave;
using Xunit;

namespace IPInterface.Test
{
    public class EFTClientIPTest
    {
        private const string TestHostName = "TestHost";
        private const int TestHostPort = 1;

        [Fact]
        public void Connect_SetsPropsEventHandlersAndCallsStart()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object)
            {
                HostName = TestHostName,
                HostPort = TestHostPort,
                UseSSL = true,
                UseKeepAlive = true
            };

            eftClient.Connect();

            Assert.Equal(TestHostName, mockTcpSocket.Object.HostName);
            Assert.Equal(TestHostPort, mockTcpSocket.Object.HostPort);
            Assert.True(mockTcpSocket.Object.UseSSL);
            Assert.True(mockTcpSocket.Object.UseKeepAlive);
            mockTcpSocket.Verify(s => s.Start(), Times.Once);
            mockTcpSocket.VerifyAdd(s => s.OnTerminated += It.IsAny<TcpSocketEventHandler>(), Times.Once);
            mockTcpSocket.VerifyAdd(s => s.OnDataWaiting += It.IsAny<TcpSocketEventHandler>(), Times.Once);
            mockTcpSocket.VerifyAdd(s => s.OnError += It.IsAny<TcpSocketEventHandler>(), Times.Once);
            mockTcpSocket.VerifyAdd(s => s.OnSend += It.IsAny<TcpSocketEventHandler>(), Times.Once);
        }

        [Fact]
        public void Disconnect_CallsStop()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.Disconnect();

            mockTcpSocket.Verify(s => s.Stop(), Times.Once);

        }


        [Fact]
        public void ClearRequestInProgress_AllowsSendingSecondMessage()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            eftClient.ClearRequestInProgress();
            var actual = eftClient.DoLogon();

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Exactly(2));
            Assert.Equal(2, sendMessageCount);
        }

        #region CheckConnectState

        [Fact]
        public void CheckConnectState_Connected_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object)
            {
                HostName = TestHostName,
                HostPort = TestHostPort,
                UseSSL = true,
                UseKeepAlive = true
            };

            eftClient.Connect();

            Assert.True(eftClient.CheckConnectState());
        }

        [Fact]
        public void CheckConnectState_Disconnected_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockDisconnectedTcpSocket();
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object)
            {
                HostName = TestHostName,
                HostPort = TestHostPort,
                UseSSL = true,
                UseKeepAlive = true
            };

            Assert.False(eftClient.CheckConnectState());

        }

        #endregion

        #region IsConnected

        [Fact]
        public void IsConnected_Connected_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object)
            {
                HostName = TestHostName,
                HostPort = TestHostPort,
                UseSSL = true,
                UseKeepAlive = true
            };

            eftClient.Connect();

            Assert.True(eftClient.IsConnected);
        }

        [Fact]
        public void IsConnected_Disconnected_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockDisconnectedTcpSocket();
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object)
            {
                HostName = TestHostName,
                HostPort = TestHostPort,
                UseSSL = true,
                UseKeepAlive = true
            };

            Assert.False(eftClient.IsConnected);

        }

        #endregion

        #region Dispose

        [Fact]
        public void Dispose_CallsDisposeOnSocket()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);
            eftClient.Dispose();

            mockTcpSocket.Verify(s => s.Dispose(), Times.Once);
        }

        #endregion

        #region DoRequest

        [Theory]
        [ClassData(typeof(EftRequestsAndGeneratedStrings))]
        public void DoRequest_Connected_SendsCorrectString_ReturnsTrue(EFTRequest request, string expectedSent)
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true);
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);
            eftClient.Connect();

            var actual = eftClient.DoRequest(request);

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(expectedSent), Times.Once);
        }

        #endregion

        #region DoLogon

        [Fact]
        public void DoLogon_SendsValidLogonRequest_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);
            var actual = eftClient.DoLogon();

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoLogon_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoLogon();

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoTransaction
        [Fact]
        public void DoTransaction_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoTransaction(new EFTTransactionRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoTransaction_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoTransaction(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoTransaction_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoTransaction(new EFTTransactionRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoGetLastTransaction

        [Fact]
        public void DoGetLastTransaction_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoGetLastTransaction(new EFTGetLastTransactionRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoGetLastTransaction_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoGetLastTransaction(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoGetLastTransaction_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoGetLastTransaction(new EFTGetLastTransactionRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoDuplicateReceipt

        [Fact]
        public void DoDuplicateReceipt_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoDuplicateReceipt(new EFTReprintReceiptRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoDuplicateReceipt_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoDuplicateReceipt(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoDuplicateReceipt_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoDuplicateReceipt(new EFTReprintReceiptRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region SendKey

        [Fact]
        public void DoSendKey_EFTPOSKey_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoSendKey(EFTPOSKey.Barcode);

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoSendKey_EFTSendKeyRequest_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoSendKey(new EFTSendKeyRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoSendKey_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoSendKey(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoSendKey_EFTSendKeyRequest_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoSendKey(new EFTSendKeyRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoSendKey_EFTPOSKey_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoSendKey(EFTPOSKey.OkCancel);

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoDisplayControlPanel

        [Fact]
        public void DoDisplayControlPanel_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoDisplayControlPanel(new EFTControlPanelRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoDisplayControlPanel_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoDisplayControlPanel(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoDisplayControlPanel_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoDisplayControlPanel(new EFTControlPanelRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoSettlement

        [Fact]
        public void DoSettlement_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoSettlement(new EFTSettlementRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoSettlement_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoSettlement(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoSettlement_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoSettlement(new EFTSettlementRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoStatus

        [Fact]
        public void DoStatus_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoStatus(new EFTStatusRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoStatus_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoStatus(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoStatus_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoStatus(new EFTStatusRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoChequeAuth

        [Fact]
        public void DoChequeAuth_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoChequeAuth(new EFTChequeAuthRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoChequeAuth_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoChequeAuth(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoChequeAuth_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoChequeAuth(new EFTChequeAuthRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoGetPassword

        [Fact]
        public void DoGetPassword_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoGetPassword(new EFTGetPasswordRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoGetPassword_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoGetPassword(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoGetPassword_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoGetPassword(new EFTGetPasswordRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoQueryCard

        [Fact]
        public void DoQueryCard_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoQueryCard(new EFTQueryCardRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoQueryCard_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoQueryCard(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoQueryCard_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoQueryCard(new EFTQueryCardRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoSlaveCommand

        [Fact]
        public void DoSlave_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoSlaveCommand("TEST");

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }


        [Fact]
        public void DoSlave_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoSlaveCommand("TEST");

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoConfigMerchant

        [Fact]
        public void DoConfigMerchant_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoConfigMerchant(new EFTConfigureMerchantRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoConfigMerchant_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoConfigMerchant(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoConfigMerchant_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoConfigMerchant(new EFTConfigureMerchantRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoCloudLogon

        [Fact]
        public void DoCloudLogon_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoCloudLogon(new EFTCloudLogonRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoCloudLogon_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoCloudLogon(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoCloudLogon_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoCloudLogon(new EFTCloudLogonRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoCloudPairing

        [Fact]
        public void DoCloudPairing_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoCloudPairing(new EFTCloudPairRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoCloudPairing_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoCloudPairing(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoCloudPairing_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoCloudPairing(new EFTCloudPairRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region DoCloudTokenLogon

        [Fact]
        public void DoCloudTokenLogon_TriggersSocketSend_ReturnsTrue()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);


            var actual = eftClient.DoCloudTokenLogon(new EFTCloudTokenLogonRequest());

            Assert.True(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        [Fact]
        public void DoCloudTokenLogon_NullRequest_ThrowsNullReferenceException()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) =>
                {
                    sendMessageCount++;
                }, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;
            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            Action actual = () => eftClient.DoCloudTokenLogon(null);
            Assert.Throws<NullReferenceException>(actual);

        }

        [Fact]
        public void DoCloudTokenLogon_WhileRequestInProgress_ReturnsFalse()
        {
            var mockTcpSocket = CreateMockConnectedTcpSocket();

            // make .Send return true  to mock successful sending
            // simulate raising an event when Send is called, increment sendMessageCount when it is raised
            int sendMessageCount = 0;
            mockTcpSocket.Setup(s => s.Send(It.IsAny<string>()))
                .Returns(true)
                .Raises(e => e.OnSend += (sender, args) => sendMessageCount++, (string msg) => new TcpSocketEventArgs { Message = msg });

            mockTcpSocket.Object.OnSend += (sender, args) => sendMessageCount++;

            var eftClient = new EftClientIPWrapper((name, port) => mockTcpSocket.Object);

            eftClient.DoLogon();
            var actual = eftClient.DoCloudTokenLogon(new EFTCloudTokenLogonRequest());

            Assert.False(actual);
            mockTcpSocket.Verify(s => s.Send(It.IsAny<string>()), Times.Once);
            Assert.Equal(1, sendMessageCount);
        }

        #endregion

        #region Testing utils

        private Mock<ITcpSocket> CreateMockConnectedTcpSocket()
        {
            var mock = new Mock<ITcpSocket>();

            // Considers itself connected
            mock.Setup(s => s.Start()).Returns(true);
            mock.Setup(s => s.IsConnected).Returns(true);
            mock.Setup(s => s.CheckConnectState()).Returns(true);

            // set up properties that get accessed
            mock.SetupProperty(s => s.HostName);
            mock.SetupProperty(s => s.HostPort);
            mock.SetupProperty(s => s.UseKeepAlive);
            mock.SetupProperty(s => s.UseSSL);

            return mock;
        }

        private Mock<ITcpSocket> CreateMockDisconnectedTcpSocket()
        {
            var mock = new Mock<ITcpSocket>();

            // Considers itself disconnected
            mock.Setup(s => s.IsConnected).Returns(false);
            mock.Setup(s => s.CheckConnectState()).Returns(false);

            // set up properties that get accessed
            mock.SetupProperty(s => s.HostName);
            mock.SetupProperty(s => s.HostPort);
            mock.SetupProperty(s => s.UseKeepAlive);
            mock.SetupProperty(s => s.UseSSL);

            return mock;
        }


        /// <summary>
        /// Simple wrapper to allow us to call the protected overloaded constructor we want that
        /// lets us mock the TcpSocket use by the EftClientClass
        /// </summary>
        internal class EftClientIPWrapper : EFTClientIP
        {

            internal EftClientIPWrapper(CreateITcpSocketDelegate del) : base(del)
            {
            }
        }
        
        #endregion
    }
}
