using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestConfigurations;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class ListTester
    {
        private const string ListKey = "List01";

        [TestMethod]
        public void TestPushRange()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Del(ListKey);

            var items = new[] { "1000", "2000", "3000", "4000" };

            dut.RPush(ListKey, items);

            var res = dut.LRange(ListKey, 0, 4);

            Assert.IsTrue(items.SequenceEqual(res));
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