using RedisLite.Client;
using RedisLite.Client.Contracts;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class LocalHostDefaultPort
    {
        internal static ConnectionSettings AsConnectionSettings() =>
            new ConnectionSettings("127.0.0.1", 6379);

        internal static AsyncRedisClient CreateAndConnectClient() =>
            CreateAndConnectRedisClient.CreateAndConnect(AsConnectionSettings());

        internal static async Task<AsyncRedisClient> CreateAndConnectClientAsync() =>
            await CreateAndConnectRedisClient.CreateAndConnectAsync(AsConnectionSettings());
    }
}