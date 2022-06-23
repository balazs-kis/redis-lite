using System.Collections.Generic;
using System.Linq;

namespace RedisLite.Client.Contracts
{
    internal static class ConnectionStringParser
    {
        public static ConnectionSettings ParseConnectionString(string connectionString)
        {
            var elements = connectionString.Split(',').ToList();

            var (address, port) = ParseAddressAndPort(elements[0]);

            elements.RemoveAt(0);

            var parameters = new Dictionary<string, string>();

            elements.ForEach(e =>
            {
                var indexOfSeparator = e.IndexOf('=');

                parameters[e.Substring(0, indexOfSeparator).ToLower()] =
                    e.Substring(indexOfSeparator + 1);
            });

            var secret = ParseSecret(parameters);
            var receiveTimeout = ParseReceiveTimeout(parameters);
            var disableParallelExecutionChecking = ParseDisableParallelExecutionChecking(parameters);
            var sslOptions = ParseSslOptions(parameters);

            return new ConnectionSettings(
                address,
                port ?? (sslOptions.UseSsl
                    ? ConnectionSettings.DefaultSslPort
                    : ConnectionSettings.DefaultPort),
                secret,
                receiveTimeout,
                disableParallelExecutionChecking,
                sslOptions);
        }


        private static (string address, int? port) ParseAddressAndPort(
            string connectionStringFragment)
        {
            string address;
            int? port = null;

            if (connectionStringFragment.Contains(":"))
            {
                var addressAndPort = connectionStringFragment.Split(':');
                address = addressAndPort[0];
                port = int.Parse(addressAndPort[1]);
            }
            else
            {
                address = connectionStringFragment;
            }

            return (address, port);
        }

        private static string ParseSecret(
            IDictionary<string, string> parameters)
        {
            const string secretKey = "password";

            return parameters.ContainsKey(secretKey)
                ? parameters[secretKey]
                : null;
        }

        private static int ParseReceiveTimeout(
            IDictionary<string, string> parameters)
        {
            const string receiveTimeoutKey = "asynctimeout";

            return parameters.ContainsKey(receiveTimeoutKey)
                ? int.Parse(parameters[receiveTimeoutKey])
                : ConnectionSettings.DefaultReceiveTimeout;
        }

        private static bool ParseDisableParallelExecutionChecking(
            IDictionary<string, string> parameters)
        {
            const string disableParallelExecutionCheckingKey = "disableconcurrencycheck";

            return
                parameters.ContainsKey(disableParallelExecutionCheckingKey) &&
                bool.Parse(parameters[disableParallelExecutionCheckingKey]);
        }

        private static SslOptions ParseSslOptions(
            IDictionary<string, string> parameters)
        {
            const string useSslKey = "ssl";
            const string useHostNameKey = "sslhost";

            if (!parameters.ContainsKey(useSslKey))
            {
                return SslOptions.Default;
            }

            var useSsl = bool.Parse(parameters[useSslKey]);

            if (!useSsl)
            {
                return SslOptions.NoSsl();
            }

            return parameters.ContainsKey(useHostNameKey)
                ? SslOptions.UseSslWithServerName(parameters[useHostNameKey])
                : SslOptions.UseSslWithDefaultServerName();
        }
    }
}