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
        public void SelectWrongDbNumber_ThrowsException() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest => await underTest.Select(int.MaxValue))
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Exists_ReturnsValueCorrectly() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Set(Key1, Value1);
                var exists1 = await underTest.Exists(Key1);
                var exists2 = await underTest.Exists("NotPresentKey");

                return (exists1, exists2);
            })
            .Assert()
                .Validate(result => result.exists1.Should().BeTrue())
                .Validate(result => result.exists2.Should().BeFalse());

        [TestMethod]
        public void Del_DeletedSuccessfully() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Set(Key1, Value1);
                var result1 = await underTest.Get(Key1);

                await underTest.Del(Key1);
                var result2 = await underTest.Get(Key1);

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().Be(Value1))
                .Validate(result => result.result2.Should().BeNull());

        [TestMethod]
        public void FlushDb_DbFlushedSuccessfully() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Set(Key1, Value1);
                await underTest.Set(Key2, Value2);

                await underTest.FlushDb();

                var result1 = await underTest.Get(Key1);
                var result2 = await underTest.Get(Key2);

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().BeNull())
                .Validate(result => result.result2.Should().BeNull());

        [TestMethod]
        public void FlushDbAsync_DbFlushedSuccessfully() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Set(Key1, Value1);
                await underTest.Set(Key2, Value2);

                await underTest.FlushDb(true);
                await Task.Delay(50);

                var result1 = await underTest.Get(Key1);
                var result2 = await underTest.Get(Key2);

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().BeNull())
                .Validate(result => result.result2.Should().BeNull());

        [TestMethod]
        public void DbSize_ReturnsValueCorrectly() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Set(Key1, Value1);
                await underTest.Set(Key2, Value2);

                return await underTest.DbSize();
            })
            .Assert()
                .Validate(result => result.Should().Be(2));

        [TestMethod]
        public void SwapDb_ClientConnectedToCorrectDb() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Select(0);
                await underTest.Set(Key1, Value1);

                await underTest.SwapDb(0, 7);

                var existsOnDb0 = await underTest.Exists(Key1);

                await underTest.Select(7);
                var readValueFromDb7 = await underTest.Get(Key1);

                return (existsOnDb0, readValueFromDb7);
            })
            .Assert()
                .Validate(result => result.existsOnDb0.Should().BeFalse())
                .Validate(result => result.readValueFromDb7.Should().Be(Value1));

        [TestMethod]
        public void SwapWithWrongDbNumbers_ThrowsException() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest => await underTest.SwapDb(int.MaxValue, int.MaxValue - 1))
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Keys_ReturnsAllKeysKeysCorrectly() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Set(Key1, Value1);
                await underTest.Set(Key2, Value2);

                return (await underTest.Keys("*")).ToList();
            })
            .Assert()
                .Validate(result => result.Count.Should().Be(2))
                .Validate(result => result.Should().Contain(Key1).And.Contain(Key2));

        [TestMethod]
        public void Keys_ReturnsMatchingKeysKeysCorrectly() => Test
            .ArrangeAsync(async () => await CreateAndConnectRedisClientAsync())
            .ActAsync(async underTest =>
            {
                await underTest.Set(Key1, Value1);
                await underTest.Set(Key2, Value2);

                return (await underTest.Keys("*1")).ToList();
            })
            .Assert()
                .Validate(result => result.Count.Should().Be(1))
                .Validate(result => result.Should().Contain(Key1));

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