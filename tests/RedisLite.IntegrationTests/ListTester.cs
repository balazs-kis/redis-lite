using RedisLite.Client.Exceptions;

namespace RedisLite.IntegrationTests
{
    [TestClass]
    public class ListTester : TestBase
    {
        private const string ListKey = "List001";
        private static readonly string[] ListItems = { "1000", "2000", "3000", "4000" };

        [ClassInitialize]
        public static async Task Setup(TestContext context) => await SetupTestContainerAsync();

        [ClassCleanup]
        public static async Task ClassCleanup() => await DisposeTestContainerAsync();

        [TestMethod]
        public async Task Test_RPush()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.RPush(ListKey, ListItems);
            var result = (await underTest.LRange(ListKey, 0, 100)).ToList();

            Assert.IsTrue(ListItems.SequenceEqual(result));
        }

        [TestMethod]
        public async Task TestWrongOperation_RPushThrowsException()
        {
            Exception? thrownException = null;

            var underTest = await CreateAndConnectRedisClientAsync();

            try
            {
                await underTest.Set(ListKey, ListItems[0]);
                await underTest.RPush(ListKey, ListItems);
            }
            catch (Exception? ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public async Task Test_LRange()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.RPush(ListKey, ListItems);
            var result = (await underTest.LRange(ListKey, 1, 2)).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(ListItems[1], result[0]);
            Assert.AreEqual(ListItems[2], result[1]);
        }

        [TestMethod]
        public async Task TestWrongOperation_LRangeThrowsException()
        {
            Exception? thrownException = null;

            var underTest = await CreateAndConnectRedisClientAsync();

            try
            {
                await underTest.Set(ListKey, ListItems[0]);
                await underTest.LRange(ListKey, 0, 1);
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
            var client = await CreateAndConnectRedisClientAsync();

            await client.Del(ListKey);
        }
    }
}