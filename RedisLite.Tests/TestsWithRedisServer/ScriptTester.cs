using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestConfigurations;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class ScriptTester
    {
        private const int ShaLength = 40;

        private const string ScriptWithoutParameter = "redis.call('set','foo', 'it works') " +
                                                      "return redis.call('get','foo')";

        private const string ScriptWithListResult = "local t = {} " +
                                                    "table.insert(t, 'one') " +
                                                    "table.insert(t, 'two') " +
                                                    "return t";

        private const string ScriptWithListInListResult = "redis.call('select', 9) " +
                                                          "redis.call('del', 'tmp01') " +
                                                          "redis.call('rpush', 'tmp01', '10', '20', '30', '40') " +
                                                          "local p = redis.call('lrange', 'tmp01', 0, 100) " +
                                                          "local t = {} " +
                                                          "table.insert(t, p) " +
                                                          "table.insert(t, p) " +
                                                          "return t";

        [TestMethod]
        public void TestScriptLoadAndRun_SingleString()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var sha = dut.LoadScript(ScriptWithoutParameter);

            Assert.AreEqual(ShaLength, sha.Length);

            var res = dut.EvalSha(sha, new string[0]).ToList();
            var resString = res[0];

            Assert.AreEqual("it works", resString);
        }

        [TestMethod]
        public void TestScriptLoadAndRun_List()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var sha = dut.LoadScript(ScriptWithListResult);

            Console.WriteLine(sha);

            var res = dut.EvalSha(sha, new string[0]);
            var resList = res as object[];

            Assert.AreEqual(2, resList?.Length);
            Assert.AreEqual("one", resList?[0] as string);
            Assert.AreEqual("two", resList?[1] as string);
        }

        [TestMethod]
        public void TestScriptLoadAndRun_ListInList()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var sha = dut.LoadScript(ScriptWithListInListResult);

            Console.WriteLine(sha);

            var result = dut.EvalSha(sha, new string[0]).ToArray();

            Assert.AreEqual(2, result?.Length);

            var embeddedArray1 = result[0] as object[];
            var embeddedArray2 = result[1] as object[];

            Assert.AreEqual("10", embeddedArray1?[0]?.ToString());
            Assert.AreEqual("20", embeddedArray1?[1]?.ToString());
            Assert.AreEqual("30", embeddedArray1?[2]?.ToString());
            Assert.AreEqual("40", embeddedArray1?[3]?.ToString());

            Assert.AreEqual("10", embeddedArray2?[0]?.ToString());
            Assert.AreEqual("20", embeddedArray2?[1]?.ToString());
            Assert.AreEqual("30", embeddedArray2?[2]?.ToString());
            Assert.AreEqual("40", embeddedArray2?[3]?.ToString());
        }


        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var dut = new RedisClient();

                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

                dut.Del("foo");

                dut.Select(9);
                dut.Del("tmp01");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}