using RedisLite.Client;
using RedisLite.Client.Contracts;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class LocalHostPort7000
    {
        internal static ConnectionSettings AsConnectionSettings() =>
            new ConnectionSettings("127.0.0.1", 7000);

        internal static AsyncRedisClient CreateAndConnectClient() =>
            CreateAndConnectRedisClient.CreateAndConnect(AsConnectionSettings());
    }
}