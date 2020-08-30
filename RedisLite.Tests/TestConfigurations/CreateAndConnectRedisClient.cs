using RedisLite.Client;
using RedisLite.Client.Contracts;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class CreateAndConnectRedisClient
    {
        public static AsyncRedisClient CreateAndConnect(ConnectionSettings settings)
        {
            var client = new AsyncRedisClient();
            client.Connect(settings).GetAwaiter().GetResult();

            return client;
        }

        public static async Task<AsyncRedisClient> CreateAndConnectAsync(ConnectionSettings settings)
        {
            var client = new AsyncRedisClient();
            await client.Connect(settings);

            return client;
        }
    }
}