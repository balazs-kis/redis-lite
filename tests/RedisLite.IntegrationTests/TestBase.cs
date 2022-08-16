using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using RedisLite.Client;
using RedisLite.Client.Contracts;

namespace RedisLite.IntegrationTests
{
    public class TestBase
    {
        private static RedisTestcontainer? _redisTestcontainer;

        protected static string? RedisConnectionString { get; set; }

        protected static ConnectionSettings? RedisConnectionSettings { get; set; }

        protected static ConnectionSettings UnknownHostConnectionSettings =>
            new("host.not.correct", 9999);

        protected static ConnectionSettings WrongPortConnectionSettings =>
            new(RedisConnectionSettings.Address, RedisConnectionSettings.Port + 100);

        protected static async Task<AsyncRedisClient> CreateAndConnectRedisClientAsync()
        {
            var client = new AsyncRedisClient();
            await client.Connect(RedisConnectionSettings);

            return client;
        }

        protected static async Task SetupTestContainerAsync()
        {
            _redisTestcontainer = new TestcontainersBuilder<RedisTestcontainer>()
                .WithDatabase(new RedisTestcontainerConfiguration())
                .Build();

            await _redisTestcontainer.StartAsync();

            RedisConnectionString = _redisTestcontainer.ConnectionString;
            RedisConnectionSettings = ConnectionSettings.FromConnectionString(RedisConnectionString);
        }

        protected static async Task DisposeTestContainerAsync()
        {
            try
            {
                if (_redisTestcontainer != null)
                {
                    await _redisTestcontainer.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}