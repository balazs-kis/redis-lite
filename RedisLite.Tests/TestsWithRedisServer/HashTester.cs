using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class HashTester : TestBase
    {
        private readonly List<string> _keys =
            Enumerable
                .Range(1, 4)
                .Select(i => Guid.NewGuid().ToString("N"))
                .ToList();

        private const string Field1 = "h1";
        private const string Value1 = "6XnJ0WvNNX2k8GXn";
        private const string Field2 = "h2";
        private const string Value2 = "ydNgA3xGuTFV8sFt";
        private const string Field3 = "h3";
        private const string Value3 = "X5DwYJaVUuVEb8m6";

        [ClassInitialize]
        public static async Task Setup(TestContext context) => await SetupTestContainerAsync();

        [ClassCleanup]
        public static async Task ClassCleanup() => await DisposeTestContainerAsync();

        [TestMethod]
        public async Task Test_SetAndGetHash()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.HSet(_keys[0], Field1, Value1);
            var res = await underTest.HGet(_keys[0], Field1);

            Assert.AreEqual(Value1, res);
        }

        [TestMethod]
        public async Task Test_SetMultipleHash()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            var additionalFields = new Dictionary<string, string>
            {
                {Field1, Value1},
                {Field2, Value2},
                {Field3, Value3}
            };

            await underTest.HMSet(_keys[1], additionalFields);

            var res1 = await underTest.HGet(_keys[1], Field1);
            var res2 = await underTest.HGet(_keys[1], Field2);
            var res3 = await underTest.HGet(_keys[1], Field3);

            Assert.AreEqual(Value1, res1);
            Assert.AreEqual(Value2, res2);
            Assert.AreEqual(Value3, res3);
        }

        [TestMethod]
        public async Task Test_GetMultipleHash()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.HSet(_keys[2], Field1, Value1);
            await underTest.HSet(_keys[2], Field2, Value2);
            await underTest.HSet(_keys[2], Field3, Value3);

            var res = (await underTest.HMGet(_keys[2], new[] { Field1, Field2, Field3 })).ToList();

            Assert.AreEqual(Value1, res[0]);
            Assert.AreEqual(Value2, res[1]);
            Assert.AreEqual(Value3, res[2]);
        }

        [TestMethod]
        public async Task Test_GetAllHash()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.HSet(_keys[3], Field1, Value1);
            await underTest.HSet(_keys[3], Field2, Value2);
            await underTest.HSet(_keys[3], Field3, Value3);

            var res = await underTest.HGetAll(_keys[3]);

            Assert.AreEqual(3, res.Count);
            Assert.AreEqual(Value1, res[Field1]);
            Assert.AreEqual(Value2, res[Field2]);
            Assert.AreEqual(Value3, res[Field3]);
        }

        [TestMethod]
        public async Task Test_GetAllHash_Null()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            var res = await underTest.HGetAll(_keys[3]);

            Assert.AreEqual(0, res.Count);
        }

        [TestMethod]
        public async Task Test_GeHash_Null()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            var res = (await underTest.HMGet(_keys[3], new[] { Field1, Field2 })).ToList();

            Assert.AreEqual(2, res.Count);
            Assert.IsNull(res[0]);
            Assert.IsNull(res[1]);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            try
            {
                var client = await CreateAndConnectRedisClientAsync();

                await client.Select(0);
                _keys.ForEach(k => client.Del(k).GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}