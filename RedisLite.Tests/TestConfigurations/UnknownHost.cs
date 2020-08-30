using RedisLite.Client;
using RedisLite.Client.Contracts;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class UnknownHost
    {
        internal static ConnectionSettings AsConnectionSettings() =>
            new ConnectionSettings("host.not.correct", 9999);

        internal static AsyncRedisClient CreateAndConnectClient() =>
            CreateAndConnectRedisClient.CreateAndConnect(AsConnectionSettings());

        internal static async Task<AsyncRedisClient> CreateAndConnectClientAsync() =>
            await CreateAndConnectRedisClient.CreateAndConnectAsync(AsConnectionSettings());
    }
}