using System;
using RedisLite.Client.Utils;

namespace RedisLite.Client.Contracts
{
    public class ConnectionSettings
    {
        public const int DefaultPort = 6379;
        public const int DefaultSslPort = 6380;
        public const int DefaultReceiveTimeout = 5000;
        public const bool DefaultParallelExecutionCheckingDisabled = false;

        public ConnectionSettings(
            string address,
            int port = DefaultPort,
            string secret = null,
            int receiveTimeoutMs = DefaultReceiveTimeout,
            bool disableParallelExecutionChecking = DefaultParallelExecutionCheckingDisabled,
            SslOptions sslOptions = null)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be null, empty or whitespace", nameof(address));
            }

            Address = address;
            Port = port;
            Secret = secret;

            Authenticate = !string.IsNullOrWhiteSpace(secret);
            ReceiveTimeout = TimeSpan.FromMilliseconds(receiveTimeoutMs);
            DisableParallelExecutionChecking = disableParallelExecutionChecking;
            SslOptions = sslOptions ?? SslOptions.Default;
        }

        public string Address { get; }
        public int Port { get; }
        public bool Authenticate { get; }
        public string Secret { get; }
        public TimeSpan ReceiveTimeout { get; }
        public bool DisableParallelExecutionChecking { get; }
        public SslOptions SslOptions { get; }

        /// <summary>
        /// Creates a <see cref="ConnectionSettings"/> instance from a connection string.
        /// Available parameters in the connection string (they are case-insensitive): password={string}, ssl={bool}, sslHost={string}, asyncTimeout={int}, disableConcurrencyCheck={bool}.
        /// Example: "redis.my-server.com:6379,password=SecurePassword,ssl=True".
        /// </summary>
        /// <param name="connectionString">The connection string to parse</param>
        /// <returns>The <see cref="ConnectionSettings"/> instance that can be used to create a client</returns>
        public static ConnectionSettings FromConnectionString(string connectionString) =>
            ConnectionStringParser.ParseConnectionString(connectionString);
    }
}