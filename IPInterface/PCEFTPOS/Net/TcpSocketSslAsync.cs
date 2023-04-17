using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace PCEFTPOS.EFTClient.IPInterface
{
    class TcpSocketSslAsync : ITcpSocketAsync
    {
        TcpClient _client = null;
        SslStream _clientStream = null;

        public async Task<bool> ConnectAsync(string hostName, int hostPort, bool keepAlive = true)
        {
            try
            {
                // Connect client 
                _client = new TcpClient();
                _client.Client.SetKeepAlive(keepAlive, 60000UL /*60 secconds*/, 1000UL /*1 sec*/);
                await _client.ConnectAsync(hostName, hostPort);

                var validator = new SslValidator(Log);

                // If we are using SSL, create the SSL stream
                _clientStream = new SslStream(_client.GetStream(), true, validator.RemoteCertificateValidationCallback);
                await _clientStream.AuthenticateAsClientAsync(hostName);

                if (_clientStream.IsAuthenticated && _clientStream.IsEncrypted && _clientStream.IsSigned)
                {
                    return true;
                }
                else
                {
                    throw new AuthenticationException("Server and client's security protocol doesn't match.");
                }
            }
            catch
            {
                Close();
                throw;
            }
        }

        /// <summary>
        /// Polls the socket to determine the current connect state
        /// </summary>
        /// <returns>True if connected, false otherwise</returns>
        public async Task<bool> CheckConnectStateAsync()
        {
            // TcpClient.Connected returns the state of the last send/recv operation. It doesn't accurately 
            // reflect the current socket state. To get the current state we need to send a packet
            try
            {
                // Check if the state is disconnected based on the last operation
                if (_client?.Connected != true || _clientStream == null)
                    return false;

                // Otherwise the socket was connected the last time we used it, send 0 byte packet to see if it still is...
                await _clientStream.WriteAsync(new byte[1], 0, 0);
            }
            catch (System.IO.IOException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.InnerException is SocketException && (e.InnerException as SocketException).NativeErrorCode == 10035)
                    return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


        public async Task<bool> WriteRequestAsync(string msgString)
        {
            // Build request
            var requestBytes = DirectEncoding.DIRECT.GetBytes(msgString);

            try
            {
                // Send the request string to the IP client.
                await _clientStream.WriteAsync(requestBytes, 0, requestBytes.Length);
            }
            catch
            {
                Close();
                throw;
            }
            return true;

        }

        public async Task<int> ReadResponseAsync(byte[] buffer, System.Threading.CancellationToken token)
        {
            try
            {
                // If we aren't using a CancellationToken we can just call ReadAsync directly
                if (token == System.Threading.CancellationToken.None)
                {
                    return await _clientStream.ReadAsync(buffer, 0, buffer.Length, token);
                }
                // Else we need to jump through some hoops as ReadAsync doesn't return when token is cancelled
                else
                {
                    var readTask = _clientStream.ReadAsync(buffer, 0, buffer.Length);

                    // FN-1558
                    // Made a change here to prevent a memory leak
                    // Instead of creating a new Task = Delay.Task(int.MaxValue, token) and adding this into
                    // the task array passed into ContinueWithAny, we pass the token as a 3rd argument 
                    // into ContineWhenAny and remove the Delay.Task
                    // A memory leak was observed when the CancellationTokeSource was disposed of 
                    // but the former Delay.Task was still running
                    return await Task.Factory.ContinueWhenAny<int>(new Task[] { readTask }, (completedTask) =>
                    {
                        return readTask.Result;
                    }, token);
                }
            }
            catch
            {
                Close();
                throw;
            }
        }

        public void Close()
        {
            _clientStream?.Close();
            if (_client?.Client != null)
            {
                _client?.Close();
            }
        }
        void Log(LogLevel level, Action<TraceRecord> traceAction)
        {
            // Check if this log level is enabled and client is subscribed to the OnLog event
            if (OnLog == null || this.LogLevel >= level)
            {
                return;
            }

            var tr = new TraceRecord() { Level = level };
            traceAction(tr);
            OnLog?.Invoke(this, new LogEventArgs() { LogLevel = level, Message = tr.Message, Exception = tr.Exception });
        }

        /// <summary>
        /// Returns the connected state as of the last read or write operation. This does not necessarily represent 
        /// the current state of the connection. 
        /// To check the current socket state call <see cref="CheckConnectStateAsync()"/>
        /// </summary>
        public bool IsConnected => _client?.Client != null && (_client?.Connected ?? false);

        /// <summary> Defines the level of logging that should be passed back in the OnLog event. Default <see cref="LogLevel.Off" />. <para>See <see cref="LogLevel"/></para></summary>
        public LogLevel LogLevel { get; set; }

        /// <summary> The log event to be called </summary>
        public event EventHandler<LogEventArgs> OnLog;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
