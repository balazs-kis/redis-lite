using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.Setup;
using System.Linq;

namespace RedisLite.Tests
{
    [TestClass]
    public class ListTester
    {
        private const string ListKey = "List01";

        [TestMethod]
        public void TestPushRange()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.ConnectionSettings);

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

            dut.Connect(LocalHostDefaultPort.ConnectionSettings);

            dut.Del(ListKey);
        }
    }
}