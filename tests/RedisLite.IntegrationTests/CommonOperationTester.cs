using RedisLite.Client;
using RedisLite.Client.Exceptions;
using System.Net.Sockets;

namespace RedisLite.IntegrationTests
{
    [TestClass]
    public class CommonOperationTester : TestBase
    {
        private const string Key1 = "TestKey1";
        private const string Value1 = "TestValue1";

        private const string Key2 = "TestKey2";
        private const string Value2 = "TestValue2";

        [ClassInitialize]
        public static async Task Setup(TestContext context) => await SetupTestContainerAsync();

        [ClassCleanup]
        public static async Task ClassCleanup() => await DisposeTestContainerAsync();

        [TestMethod]
        public void Connect_ConnectsSuccessfully() => Test
            .Arrange(() => new AsyncRedisClient())
            .ActAsync(async underTest => await underTest.Connect(RedisConnectionSettings))
            .Assert().IsSuccess();

        [TestMethod]
        public void ConnectToUnknownHost_ThrowsException() => Test
            .Arrange(() => new AsyncRedisClient())
            .ActAsync(async underTest => await underTest.Connect(UnknownHostConnectionSettings))
            .Assert().ThrewException<SocketException>();

        [TestMethod]
        public void ConnectToKnownHostWrongPort_ThrowsException() => Test
            .Arrange(() => new AsyncRedisClient())
            .ActAsync(async underTest => await underTest.Connect(WrongPortConnectionSettings))
            .Assert().ThrewException<SocketException>();

        [TestMethod]
        public void ConnectCalledTwice_ThrowsException() => Test
            .Arrange(() => new AsyncRedisClient())
            .ActAsync(async underTest =>
            {
                await underTest.Connect(RedisConnectionSettings);
                await underTest.Connect(RedisConnectionSettings);
            })
            .Assert().ThrewException<InvalidOperationException>();

        [TestMethod]
        public void CallingClientAfterDispose_ThrowsException() => Test
            .Arrange(() => new AsyncRedisClient())
            .ActAsync(async underTest =>
            {
                underTest.Dispose();
                await underTest.Set(Key1, Value1);
            })
            .Assert().ThrewException<InvalidOperationException>();

        [TestMethod]
        public void Select_ClientSelectsNewDb() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Select(7);
                await underTest.Set(Key1, Value1);
                var fromDb7 = await underTest.Get(Key1);
                await underTest.Select(8);
                var fromDb8 = await underTest.Get(Key1);

                return (fromDb7, fromDb8);
            })
            .Assert()
                .Validate(result => result.fromDb7.Should().Be(Value1))
                .Validate(result => result.fromDb8.Should().BeNull());

        [TestMethod]
        public async Task SelectWrongDbNumber_ThrowsException()
        {
            Exception thrownException = null;
            var underTest = await CreateAndConnectRedisClientAsync();

            try
            {
                await underTest.Select(int.MaxValue);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            thrownException.Should().NotBeNull().And.BeOfType<RedisException>();
        }

        [TestMethod]
        public async Task Exists_ReturnsValueCorrectly()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.Set(Key1, Value1);
            var result1 = await underTest.Exists(Key1);
            var result2 = await underTest.Exists("NotPresentKey");

            result1.Should().BeTrue();
            result2.Should().BeFalse();
        }

        [TestMethod]
        public async Task Del_DeletedSuccessfully()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.Set(Key1, Value1);
            var result1 = await underTest.Get(Key1);
            await underTest.Del(Key1);
            var result2 = await underTest.Get(Key1);

            result1.Should().Be(Value1);
            result2.Should().BeNull();
        }

        [TestMethod]
        public async Task FlushDb_DbFlushedSuccessfully()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.Set(Key1, Value1);
            await underTest.Set(Key2, Value2);
            await underTest.FlushDb();
            var result1 = await underTest.Get(Key1);
            var result2 = await underTest.Get(Key2);

            result1.Should().BeNull();
            result2.Should().BeNull();
        }

        [TestMethod]
        public async Task FlushDbAsync_DbFlushedSuccessfully()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.Set(Key1, Value1);
            await underTest.Set(Key2, Value2);
            await underTest.FlushDb(true);
            Thread.Sleep(50);
            var result1 = await underTest.Get(Key1);
            var result2 = await underTest.Get(Key2);

            result1.Should().BeNull();
            result2.Should().BeNull();
        }

        [TestMethod]
        public async Task DbSize_ReturnsValueCorrectly()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.Set(Key1, Value1);
            await underTest.Set(Key2, Value2);
            var result = await underTest.DbSize();

            result.Should().Be(2);
        }

        [TestMethod]
        public async Task SwapDb_ClientConnectedToCorrectDb()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.Select(0);
            await underTest.Set(Key1, Value1);
            await underTest.SwapDb(0, 7);
            var existsOnDb0 = await underTest.Exists(Key1);
            await underTest.Select(7);
            var readValueFromDb7 = await underTest.Get(Key1);

            existsOnDb0.Should().BeFalse();
            readValueFromDb7.Should().Be(Value1);
        }

        [TestMethod]
        public async Task SwapWithWrongDbNumbers_ThrowsException()
        {
            Exception thrownException = null;
            var underTest = await CreateAndConnectRedisClientAsync();

            try
            {
                await underTest.SwapDb(int.MaxValue, int.MaxValue - 1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            thrownException.Should().NotBeNull().And.BeOfType<RedisException>();
        }

        [TestMethod]
        public async Task Keys_ReturnsKeysCorrectly()
        {
            var underTest = await CreateAndConnectRedisClientAsync();

            await underTest.Set(Key1, Value1);
            await underTest.Set(Key2, Value2);
            var result = await underTest.Keys("*");
            var resultList = result.ToList();

            resultList.Count.Should().Be(2);
            resultList.Should().Contain(Key1).And.Contain(Key2);
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            try
            {
                var client = await CreateAndConnectRedisClientAsync();

                await client.Select(0);
                await client.Del(Key1);
                await client.Del(Key2);

                await client.Select(7);
                await client.Del(Key1);
                await client.Del(Key2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}