using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using System;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestsWithRedisServer
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
        public async Task Test_ConnectedEvent()
        {
            var dut = new AsyncRedisClient();

            string result = null;

            dut.OnConnected += async c =>
            {
                await c.Set(Key, Value);
                result = await dut.Get(Key);
            };

            await dut.Connect(RedisConnectionSettings);
            await Task.Delay(1250);

            Assert.AreEqual(Value, result);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            try
            {
                var dut = new AsyncRedisClient();

                await dut.Connect(RedisConnectionSettings);

                await dut.Del(Key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}