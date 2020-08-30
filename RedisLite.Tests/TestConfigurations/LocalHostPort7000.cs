using RedisLite.Client;
using RedisLite.Client.Contracts;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class LocalHostPort7000
    {
        internal static ConnectionSettings AsConnectionSettings() =>
            new ConnectionSettings("127.0.0.1", 7000);

        internal static AsyncRedisClient CreateAndConnectClient() =>
            CreateAndConnectRedisClient.CreateAndConnect(AsConnectionSettings());

        internal static async Task<AsyncRedisClient> CreateAndConnectClientAsync() =>
            await CreateAndConnectRedisClient.CreateAndConnectAsync(AsConnectionSettings());
    }
}