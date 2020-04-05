using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestConfigurations;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class CommonOperationTester
    {
        private const string Key = "TestKey";
        private const string Value = "TestValue";

        private const string Key2 = "TestKey2";
        private const string Value2 = "TestValue2";

        [TestMethod]
        public void Test_Select()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Select(7);
            dut.Set(Key, Value);
            var res1 = dut.Get(Key);

            dut.Select(8);
            var res2 = dut.Get(Key);

            Assert.AreEqual(Value, res1);
            Assert.IsNull(res2);
        }

        [TestMethod]
        public void Test_Exists()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Set(Key, Value);
            var res1 = dut.Exists(Key);
            var res2 = dut.Exists("NotPresentKey");

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
        }

        [TestMethod]
        public void Test_Del()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Set(Key, Value);
            var res1 = dut.Get(Key);

            dut.Del(Key);
            var res2 = dut.Get(Key);

            Assert.AreEqual(Value, res1);
            Assert.IsNull(res2);
        }

        [TestMethod]
        public void Test_FlushAndDbSize()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.FlushDb();
            dut.Set(Key, Value);
            dut.Set(Key2, Value2);
            var res1 = dut.DbSize();

            dut.FlushDb();
            var res2 = dut.DbSize();

            Assert.AreEqual(2, res1);
            Assert.AreEqual(0, res2);
        }

        [TestMethod]
        public void Test_SwapDb()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Select(0);
            dut.Set(Key, Value);
            dut.SwapDb(0, 7);
            var existsOnDb0 = dut.Exists(Key);
            dut.Select(7);
            var result = dut.Get(Key);
            
            Assert.IsFalse(existsOnDb0);
            Assert.AreEqual(Value, result);
        }


        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var dut = new RedisClient();
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

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