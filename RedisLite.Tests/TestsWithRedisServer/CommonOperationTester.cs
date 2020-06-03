using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using TestLite;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Net.Sockets;
using System.Threading;

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
        public void Connect_ConnectsSuccessfully() => Test
            .Arrange(() => new RedisClient())
            .Act(underTest => underTest.Connect(LocalHostDefaultPort.AsConnectionSettings()).GetAwaiter().GetResult())
            .Assert().IsSuccess();

        [TestMethod]
        public void ConnectToUnknownHost_ThrowsException() => Test
            .Arrange(() => new RedisClient())
            .Act(underTest => underTest.Connect(UnknownHost.AsConnectionSettings()).GetAwaiter().GetResult())
            .Assert().ThrewException<SocketException>();

        [TestMethod]
        public void ConnectToKnownHostWrongPort_ThrowsException() => Test
            .Arrange(() => new RedisClient())
            .Act(underTest => underTest.Connect(LocalHostPort7000.AsConnectionSettings()).GetAwaiter().GetResult())
            .Assert().ThrewException<SocketException>();

        [TestMethod]
        public void ConnectCalledTwice_ThrowsException() => Test
            .Arrange(() => new RedisClient())
            .Act(underTest =>
            {
                underTest.Connect(LocalHostDefaultPort.AsConnectionSettings()).GetAwaiter().GetResult();
                underTest.Connect(LocalHostDefaultPort.AsConnectionSettings()).GetAwaiter().GetResult();
            })
            .Assert().ThrewException<InvalidOperationException>();

        [TestMethod]
        public void CallingClientAfterDispose_ThrowsException() => Test
            .Arrange(() =>
            {
                var client = LocalHostDefaultPort.CreateAndConnectClient();
                client.Dispose();
                return client;
            })
            .Act(underTest => underTest.Set(Key, Value))
            .Assert().ThrewException<InvalidOperationException>();

        [TestMethod]
        public void Select_ClientSelectsNewDb() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Select(7).GetAwaiter().GetResult();
                underTest.Set(Key, Value).GetAwaiter().GetResult();
                var result1 = underTest.Get(Key).GetAwaiter().GetResult();
                underTest.Select(8).GetAwaiter().GetResult();
                var result2 = underTest.Get(Key).GetAwaiter().GetResult();

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().Be(Value))
                .Validate(result => result.result2.Should().BeNull());

        [TestMethod]
        public void SelectWrongDbNumber_ThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest => underTest.Select(int.MaxValue).GetAwaiter().GetResult())
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Exists_ReturnsValueCorrectly() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(Key, Value).GetAwaiter().GetResult();
                var result1 = underTest.Exists(Key).GetAwaiter().GetResult();
                var result2 = underTest.Exists("NotPresentKey").GetAwaiter().GetResult();

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().BeTrue())
                .Validate(result => result.result2.Should().BeFalse());

        [TestMethod]
        public void Del_DeletedSuccessfully() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(Key, Value).GetAwaiter().GetResult();
                var result1 = underTest.Get(Key).GetAwaiter().GetResult();
                underTest.Del(Key).GetAwaiter().GetResult();
                var result2 = underTest.Get(Key).GetAwaiter().GetResult();

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().Be(Value))
                .Validate(result => result.result2.Should().BeNull());

        [TestMethod]
        public void FlushDb_DbFlushedSuccessfully() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(Key, Value).GetAwaiter().GetResult();
                underTest.Set(Key2, Value2).GetAwaiter().GetResult();
                underTest.FlushDb().GetAwaiter().GetResult();
                var result1 = underTest.Get(Key).GetAwaiter().GetResult();
                var result2 = underTest.Get(Key2).GetAwaiter().GetResult();

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().BeNull())
                .Validate(result => result.result2.Should().BeNull());

        [TestMethod]
        public void FlushDbAsync_DbFlushedSuccessfully() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(Key, Value).GetAwaiter().GetResult();
                underTest.Set(Key2, Value2).GetAwaiter().GetResult();
                underTest.FlushDb(true).GetAwaiter().GetResult();
                Thread.Sleep(50);
                var result1 = underTest.Get(Key).GetAwaiter().GetResult();
                var result2 = underTest.Get(Key2).GetAwaiter().GetResult();

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().BeNull())
                .Validate(result => result.result2.Should().BeNull());

        [TestMethod]
        public void DbSize_ReturnsValueCorrectly() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(Key, Value).GetAwaiter().GetResult();
                underTest.Set(Key2, Value2).GetAwaiter().GetResult();

                return underTest.DbSize();
            })
            .Assert().Validate(result => result.Should().Be(2));

        [TestMethod]
        public void SwapDb_ClientConnectedToCorrectDb() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Select(0).GetAwaiter().GetResult();
                underTest.Set(Key, Value).GetAwaiter().GetResult();
                underTest.SwapDb(0, 7).GetAwaiter().GetResult();
                var existsOnDb0 = underTest.Exists(Key).GetAwaiter().GetResult();
                underTest.Select(7).GetAwaiter().GetResult();
                var readValueFromDb7 = underTest.Get(Key).GetAwaiter().GetResult();

                return (existsOnDb0, readValueFromDb7);
            })
            .Assert()
                .Validate(result => result.existsOnDb0.Should().BeFalse())
                .Validate(result => result.readValueFromDb7.Should().Be(Value));

        [TestMethod]
        public void SwapWithWrongDbNumbers_ThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest => underTest.SwapDb(int.MaxValue, int.MaxValue - 1).GetAwaiter().GetResult())
            .Assert().ThrewException<RedisException>();

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var dut = new RedisClient();
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings()).GetAwaiter().GetResult();

                dut.Select(0).GetAwaiter().GetResult();
                dut.Del(Key).GetAwaiter().GetResult();
                dut.Del(Key2).GetAwaiter().GetResult();

                dut.Select(7).GetAwaiter().GetResult();
                dut.Del(Key).GetAwaiter().GetResult();
                dut.Del(Key2).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}