using RedisLite.Client;

namespace RedisLite.IntegrationTests
{
    [TestClass]
    public class EventTester : TestBase
    {
        private const string Key = "TestKey";
        private const string Value = "TestValue";

        [ClassInitialize]
        public static async Task Setup(TestContext context) => await SetupTestContainerAsync();

        [ClassCleanup]
        public static async Task ClassCleanup() => await DisposeTestContainerAsync();

        [TestMethod]
        public void Test_ConnectedEvent() => Test
            .Arrange(() => new AsyncRedisClient())
            .ActAsync(async underTest =>
            {
                string? result = null;

                underTest.OnConnected += async c =>
                {
                    await c.Set(Key, Value);
                    result = await underTest.Get(Key);
                };

                await underTest.Connect(RedisConnectionSettings);
                await Task.Delay(1250);

                return result;
            })
            .Assert().Validate(result => result.Should().Be(Value));

        [TestCleanup]
        public async Task Cleanup()
        {
            try
            {
                var client = await CreateAndConnectRedisClientAsync();

                await client.Del(Key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}