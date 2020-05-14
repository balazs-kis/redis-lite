using RedisLite.Client;
using RedisLite.Client.Contracts;

namespace RedisLite.Tests.TestConfigurations
{
    internal static class CreateAndConnectRedisClient
    {
        public static RedisClient CreateAndConnect(ConnectionSettings settings)
        {
            var client = new RedisClient();
            client.Connect(settings);

            return client;
        }
    }
}