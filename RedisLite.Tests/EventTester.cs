using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.Setup;
using System;

namespace RedisLite.Tests
{
    [TestClass]
    public class EventTester
    {
        private const string Key = "TestKey";
        private const string Value = "TestValue";

        [TestMethod]
        public void Test_ConnectedEvent()
        {
            var dut = new RedisClient();

            string result = null;

            dut.OnConnected += c =>
            {
                c.Set(Key, Value);
                result = dut.Get(Key);
            };

            dut.Connect(LocalHostDefaultPort.ConnectionSettings);

            Assert.AreEqual(Value, result);
        }


        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var dut = new RedisClient();

                dut.Connect(LocalHostDefaultPort.ConnectionSettings);

                dut.Del(Key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }

    }
}