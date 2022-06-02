using System;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <exclude/>
    public class TcpSocketException : Exception
    {

        /// <exclude/>
        public TcpSocketException(TcpSocketExceptionType ExceptionType, string Message)
            : base(Message)
        {
            this.ExceptionType = ExceptionType;
        }

        /// <exclude/>
        public TcpSocketExceptionType ExceptionType { get; private set; }
    }

    /// <exclude/>
    public enum TcpSocketExceptionType
    {
        /// <exclude/>
        ConnectException,
        /// <exclude/>
        GeneralException,
        /// <exclude/>
        SendException,
        /// <exclude/>
        ReceiveException
    }

    /// <exclude/>
    public class TcpSocketEventArgs : EventArgs
    {
        /// <exclude/>
        public string Error { get; set; }
        /// <exclude/>
        public TcpSocketExceptionType ExceptionType { get; set; }
        /// <exclude/>
        public byte[] Bytes { get; set; }
        /// <exclude/>
        public string Message { get; set; }
    }

    /// <exclude/>
    public delegate void TcpSocketEventHandler(object sender, TcpSocketEventArgs e);

    /// <summary>
    /// Defines the socket interface used by EFTClientIP
    /// </summary>
    public interface ITcpSocket : IDisposable
    {
        event TcpSocketEventHandler OnDataWaiting;
        event TcpSocketEventHandler OnTerminated;
        event TcpSocketEventHandler OnError;
        event TcpSocketEventHandler OnSend;

        bool Start();
        void Stop();
        bool Send(string message);
        string HostName { get; set; }
        int HostPort { get; set; }
        bool UseSSL { get; set; }
        bool UseKeepAlive { get; set; }

        /// <summary>
        /// Polls the socket to determine the current connect state
        /// </summary>
        /// <returns>True if connected, false otherwise</returns>
        bool CheckConnectState();

        /// <summary>
        /// Returns the connected state as of the last read or write operation. This does not necessarily represent 
        /// the current state of the connection. 
        /// To check the current socket state call <see cref="CheckConnectState()"/>
        /// </summary>
        bool IsConnected { get; }

    }
}
