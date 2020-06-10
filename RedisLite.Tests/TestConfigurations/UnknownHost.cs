using RedisLite.Client;
using RedisLite.Client.Contracts;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class UnknownHost
    {
        internal static ConnectionSettings AsConnectionSettings() =>
            new ConnectionSettings("host.not.correct", 9999);

        internal static AsyncRedisClient CreateAndConnectClient() =>
            CreateAndConnectRedisClient.CreateAndConnect(AsConnectionSettings());
    }
}