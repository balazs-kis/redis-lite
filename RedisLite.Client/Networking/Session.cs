using RedisLite.Client.Encoding;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RedisLite.Client.Networking
{
    internal class Session : ISession
    {
        private const int BufferSize = 1024;

        private bool _disposed;
        private TcpClient _client;
        private NetworkStream _networkStream;
        private SslStream _sslStream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;

        public bool IsOpen { get; private set; }

        public Locker Locker { get; }

        public StreamReader StreamReader
        {
            get
            {
                if (IsOpen)
                {
                    return _streamReader;
                }

                throw new IOException("This session has already lost connection; the StreamReader cannot be acquired");
            }
        }

        public StreamWriter StreamWriter
        {
            get
            {
                if (IsOpen)
                {
                    return _streamWriter;
                }

                throw new IOException("This session has already lost connection; the StreamWriter cannot be acquired");
            }
        }


        public Session(bool disableParallelExecutionChecking)
        {
            Locker = disableParallelExecutionChecking ? null : new Locker();
        }


        public async Task<bool> OpenAsync(
            string address,
            int port,
            TimeSpan receiveTimeout,
            bool useSsl,
            string sslServerName)
        {
            if (IsConnected())
            {
                throw new InvalidOperationException("The session was already opened");
            }

            _client = new TcpClient { NoDelay = true };
            await _client.ConnectAsync(address, port);

            _client.Client.ReceiveTimeout = _client.Client.SendTimeout = (int)receiveTimeout.TotalMilliseconds;

            _networkStream = _client.GetStream();

            _networkStream.ReadTimeout = (int)receiveTimeout.TotalMilliseconds;
            _networkStream.WriteTimeout = (int)receiveTimeout.TotalMilliseconds;

            if (useSsl)
            {
                _sslStream = new SslStream(
                    _networkStream,
                    true,
                    ValidateServerCertificate,
                    null);

                await _sslStream.AuthenticateAsClientAsync(sslServerName);

                _sslStream.ReadTimeout = (int)receiveTimeout.TotalMilliseconds;
                _sslStream.WriteTimeout = (int)receiveTimeout.TotalMilliseconds;
            }

            _streamReader = new StreamReader(
                useSsl ? (Stream)_sslStream : _networkStream,
                ContentEncoder.Encoding,
                false,
                BufferSize,
                true);

            _streamWriter = new StreamWriter(
                useSsl ? (Stream)_sslStream : _networkStream,
                ContentEncoder.Encoding,
                BufferSize,
                true);

            IsOpen = IsConnected();

            return IsOpen;
        }

        public void SetInfiniteReadTimeout()
        {
            _networkStream.ReadTimeout = -1;

            if (_sslStream != null)
            {
                _sslStream.ReadTimeout = -1;
            }
        }

        private bool IsConnected()
        {
            if (_client?.Client == null || _client.Connected == false)
            {
                return false;
            }

            var canWrite = _client.Client.Poll(0, SelectMode.SelectWrite);
            var hasError = _client.Client.Poll(0, SelectMode.SelectError);

            return canWrite && !hasError;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                IsOpen = false;

                _streamReader?.Dispose();
                _streamReader = null;
                _streamWriter?.Dispose();
                _streamWriter = null;

                if (_client.Connected)
                {
                    _client.GetStream().Close();
                    _client.Close();
                    _client.Dispose();
                }

                _client = null;
            }

            _disposed = true;
        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors) =>
            sslPolicyErrors == SslPolicyErrors.None;
    }
}