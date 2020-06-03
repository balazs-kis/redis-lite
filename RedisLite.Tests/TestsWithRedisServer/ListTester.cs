using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class ListTester
    {
        private const string ListKey = "List01";
        private static readonly string[] ListItems = { "1000", "2000", "3000", "4000" };

        [TestMethod]
        public async Task Test_RPush()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.RPush(ListKey, ListItems);
            var result = (await dut.LRange(ListKey, 0, 100)).ToList();

            Assert.IsTrue(ListItems.SequenceEqual(result));
        }

        [TestMethod]
        public async Task TestWrongOperation_RPushThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                await dut.Set(ListKey, ListItems[0]);
                await dut.RPush(ListKey, ListItems);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public async Task Test_LRange()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.RPush(ListKey, ListItems);
            var result = (await dut.LRange(ListKey, 1, 2)).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(ListItems[1], result[0]);
            Assert.AreEqual(ListItems[2], result[1]);
        }

        [TestMethod]
        public async Task TestWrongOperation_LRangeThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                await dut.Set(ListKey, ListItems[0]);
                await dut.LRange(ListKey, 0, 1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }


        [TestCleanup]
        public async Task Cleanup()
        {
            var dut = new RedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Del(ListKey);
        }
    }
}