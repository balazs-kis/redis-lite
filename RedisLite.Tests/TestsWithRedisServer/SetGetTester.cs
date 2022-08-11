using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using System;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class SetGetTester : TestBase
    {
        private const string Key = "TestKey001";
        private const string Value = "TestValue001";

        [ClassInitialize]
        public static async Task Setup(TestContext context) => await SetupTestContainerAsync();

        [ClassCleanup]
        public static async Task ClassCleanup() => await DisposeTestContainerAsync();

        [TestMethod]
        public async Task Test_Set_Get()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.Set(Key, Value);
            var res = await underTest.Get(Key);

            Assert.AreEqual(Value, res);
        }

        [TestMethod]
        public async Task TestWrongOperation_GetThrowsException()
        {
            Exception thrownException = null;

            var underTest = await CreateAndConnectRedisClientAsync();

            try
            {
                await underTest.SAdd(Key, Value);
                await underTest.Get(Key);
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

            var underTest = new AsyncRedisClient();

            try
            {
                await underTest.Get(Key);
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
            var underTest = await CreateAndConnectRedisClientAsync();

            Exception thrownException = null;
            try
            {
                await underTest.Ping();
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
                var client = await CreateAndConnectRedisClientAsync();

                await client.Select(0);
                await client.Del(Key);

                await client.Select(7);
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