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
            bool disableParallelExecutionChecking = false)
        {
            Address = address;
            Port = port;
            Secret = secret;

            Authenticate = !string.IsNullOrWhiteSpace(secret);
            ReceiveTimeout = TimeSpan.FromMilliseconds(receiveTimeoutMs);
            DisableParallelExecutionChecking = disableParallelExecutionChecking;
        }

        public string Address { get; }
        public int Port { get; }
        public bool Authenticate { get; }
        public string Secret { get; }
        public TimeSpan ReceiveTimeout { get; }
        public bool DisableParallelExecutionChecking { get; }
    }
}