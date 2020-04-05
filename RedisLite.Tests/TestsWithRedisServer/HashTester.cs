using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestConfigurations;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class HashTester
    {
        private readonly List<string> _keys =
            Enumerable
                .Repeat<object>(null, 4)
                .Select(i => Guid.NewGuid().ToString("N"))
                .ToList();

        private const string Field1 = "h1";
        private const string Value1 = "6XnJ0WvNNX2k8GXn";
        private const string Field2 = "h2";
        private const string Value2 = "ydNgA3xGuTFV8sFt";
        private const string Field3 = "h3";
        private const string Value3 = "X5DwYJaVUuVEb8m6";

        [TestMethod]
        public void Test_SetAndGetHash()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.HSet(_keys[0], Field1, Value1);
            var res = dut.HGet(_keys[0], Field1);

            Assert.AreEqual(Value1, res);
        }

        [TestMethod]
        public void Test_SetMultipleHash()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var additionalFields = new Dictionary<string, string>
            {
                {Field1, Value1},
                {Field2, Value2},
                {Field3, Value3}
            };

            dut.HMSet(_keys[1], additionalFields);

            var res1 = dut.HGet(_keys[1], Field1);
            var res2 = dut.HGet(_keys[1], Field2);
            var res3 = dut.HGet(_keys[1], Field3);

            Assert.AreEqual(Value1, res1);
            Assert.AreEqual(Value2, res2);
            Assert.AreEqual(Value3, res3);
        }

        [TestMethod]
        public void Test_GetMultipleHash()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.HSet(_keys[2], Field1, Value1);
            dut.HSet(_keys[2], Field2, Value2);
            dut.HSet(_keys[2], Field3, Value3);

            var res = dut.HMGet(_keys[2], new[] { Field1, Field2, Field3 }).ToList();

            Assert.AreEqual(Value1, res[0]);
            Assert.AreEqual(Value2, res[1]);
            Assert.AreEqual(Value3, res[2]);
        }

        [TestMethod]
        public void Test_GetAllHash()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.HSet(_keys[3], Field1, Value1);
            dut.HSet(_keys[3], Field2, Value2);
            dut.HSet(_keys[3], Field3, Value3);

            var res = dut.HGetAll(_keys[3]);

            Assert.AreEqual(3, res.Count);
            Assert.AreEqual(Value1, res[Field1]);
            Assert.AreEqual(Value2, res[Field2]);
            Assert.AreEqual(Value3, res[Field3]);
        }

        [TestMethod]
        public void Test_GetAllHash_Null()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var res = dut.HGetAll(_keys[3]);

            Assert.AreEqual(0, res.Count);
        }

        [TestMethod]
        public void Test_GeHash_Null()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            var res = dut.HMGet(_keys[3], new[] { Field1, Field2 }).ToList();

            Assert.AreEqual(2, res.Count);
            Assert.IsNull(res[0]);
            Assert.IsNull(res[1]);
        }


        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var dut = new RedisClient();
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

                dut.Select(0);
                _keys.ForEach(k => dut.Del(k));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}