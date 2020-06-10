using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class EventTester
    {
        private const string Key = "TestKey";
        private const string Value = "TestValue";

        private class StringWrapper
        {
            public string StringValue { get; set; }
        }

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

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            await Task.Delay(1250);

            Assert.AreEqual(Value, result);
        }


        [TestCleanup]
        public async Task Cleanup()
        {
            try
            {
                var dut = new AsyncRedisClient();

                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

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