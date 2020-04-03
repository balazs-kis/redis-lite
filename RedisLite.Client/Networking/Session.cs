using System;
using System.IO;
using System.Net.Sockets;
using RedisLite.Client.Encoding;

namespace RedisLite.Client.Networking
{
    internal class Session : ISession
    {
        private const int BufferSize = 1024;

        private bool _disposed;
        private TcpClient _client;
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


        public Session()
        {
            Locker = new Locker();
        }


        public bool Open(string address, int port, TimeSpan receiveTimeout)
        {
            if (IsConnected())
            {
                throw new InvalidOperationException("The session was already opened");
            }

            _client = new TcpClient { NoDelay = true };
            _client.Connect(address, port);

            _client.Client.ReceiveTimeout = _client.Client.SendTimeout = (int)receiveTimeout.TotalMilliseconds;
            var stream = _client.GetStream();

            stream.ReadTimeout = (int)receiveTimeout.TotalMilliseconds;
            stream.WriteTimeout = (int)receiveTimeout.TotalMilliseconds;

            _streamReader = new StreamReader(stream, ContentEncoder.Encoding, false, BufferSize, true);
            _streamWriter = new StreamWriter(stream, ContentEncoder.Encoding, BufferSize, true);

            IsOpen = IsConnected();

            return IsOpen;
        }

        public void SetInfiniteReadTimeout()
        {
            _client.GetStream().ReadTimeout = -1;
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
                }

                _client = null;
            }

            _disposed = true;
        }
    }
}