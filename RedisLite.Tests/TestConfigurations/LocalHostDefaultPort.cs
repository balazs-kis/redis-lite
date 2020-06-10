using RedisLite.Client;
using RedisLite.Client.Contracts;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class LocalHostDefaultPort
    {
        internal static ConnectionSettings AsConnectionSettings() =>
            new ConnectionSettings("127.0.0.1", 6379);

        internal static AsyncRedisClient CreateAndConnectClient() =>
            CreateAndConnectRedisClient.CreateAndConnect(AsConnectionSettings());
    }
}