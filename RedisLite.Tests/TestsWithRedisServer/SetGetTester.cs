using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class SetGetTester
    {
        private const string Key = "TestKey";
        private const string Value = "TestValue";

        [TestMethod]
        public async Task Test_Set_Get()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Set(Key, Value);
            var res = await dut.Get(Key);

            Assert.AreEqual(Value, res);
        }

        [TestMethod]
        public async Task TestWrongOperation_GetThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                await dut.SAdd(Key, Value);
                await dut.Get(Key);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public async Task TestUnconnectedClient_GetThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();

            try
            {
                await dut.Get(Key);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task Test_Ping()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            Exception thrownException = null;
            try
            {
                await dut.Ping();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNull(thrownException);
        }


        [TestCleanup]
        public async Task Cleanup()
        {
            try
            {
                var dut = new RedisClient();
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

                await dut.Select(0);
                await dut.Del(Key);

                await dut.Select(7);
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