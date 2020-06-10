using RedisLite.Client;
using RedisLite.Client.Contracts;

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
    }
}