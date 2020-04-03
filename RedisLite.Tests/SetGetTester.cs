using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.Setup;
using System;

namespace RedisLite.Tests
{
    [TestClass]
    public class SetGetTester
    {
        private const string Key = "TestKey";
        private const string Value = "TestValue";

        [TestMethod]
        public void Test_Set_Get()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.ConnectionSettings);

            dut.Set(Key, Value);
            var res = dut.Get(Key);

            Assert.AreEqual(Value, res);
        }

        [TestMethod]
        public void Test_Ping()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.ConnectionSettings);

            Exception thrownException = null;
            try
            {
                dut.Ping();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNull(thrownException);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var dut = new RedisClient();
                dut.Connect(LocalHostDefaultPort.ConnectionSettings);

                dut.Select(0);
                dut.Del(Key);

                dut.Select(7);
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