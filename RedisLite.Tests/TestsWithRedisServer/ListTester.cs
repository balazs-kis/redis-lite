using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Linq;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class ListTester
    {
        private const string ListKey = "List01";
        private static readonly string[] ListItems = { "1000", "2000", "3000", "4000" };

        [TestMethod]
        public void Test_RPush()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.RPush(ListKey, ListItems);
            var result = dut.LRange(ListKey, 0, 100).ToList();

            Assert.IsTrue(ListItems.SequenceEqual(result));
        }

        [TestMethod]
        public void TestWrongOperation_RPushThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.Set(ListKey, ListItems[0]);
                dut.RPush(ListKey, ListItems);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public void Test_LRange()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.RPush(ListKey, ListItems);
            var result = dut.LRange(ListKey, 1, 2).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(ListItems[1], result[0]);
            Assert.AreEqual(ListItems[2], result[1]);
        }

        [TestMethod]
        public void TestWrongOperation_LRangeThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.Set(ListKey, ListItems[0]);
                dut.LRange(ListKey, 0, 1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }


        [TestCleanup]
        public void Cleanup()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Del(ListKey);
        }
    }
}