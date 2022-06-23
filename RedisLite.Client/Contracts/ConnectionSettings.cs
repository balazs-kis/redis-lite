using System;

namespace RedisLite.Client.Contracts
{
    public class ConnectionSettings
    {
        public ConnectionSettings(
            string address,
            int port,
            string secret = null,
            int receiveTimeoutMs = 5000,
            bool disableParallelExecutionChecking = false,
            SslOptions sslOptions = null)
        {
            Address = address;
            Port = port;
            Secret = secret;

            Authenticate = !string.IsNullOrWhiteSpace(secret);
            ReceiveTimeout = TimeSpan.FromMilliseconds(receiveTimeoutMs);
            DisableParallelExecutionChecking = disableParallelExecutionChecking;
            SslOptions = sslOptions ?? SslOptions.NoSsl();
        }

        public string Address { get; }
        public int Port { get; }
        public bool Authenticate { get; }
        public string Secret { get; }
        public TimeSpan ReceiveTimeout { get; }
        public bool DisableParallelExecutionChecking { get; }
        public SslOptions SslOptions { get; }
    }
}