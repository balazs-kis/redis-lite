using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task Connect_ConnectsSuccessfully()
        {
            Exception thrownException = null;
            var underTest = new RedisClient();

            try
            {
                await underTest.Connect(LocalHostDefaultPort.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            thrownException.Should().BeNull();
        }

        [TestMethod]
        public async Task ConnectToUnknownHost_ThrowsException()
        {
            Exception thrownException = null;
            var underTest = new RedisClient();

            try
            {
                await underTest.Connect(UnknownHost.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            thrownException.Should().NotBeNull().And.BeAssignableTo<SocketException>();
        }

        [TestMethod]
        public async Task ConnectToKnownHostWrongPort_ThrowsException()
        {
            Exception thrownException = null;
            var underTest = new RedisClient();

            try
            {
                await underTest.Connect(LocalHostPort7000.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            thrownException.Should().NotBeNull().And.BeAssignableTo<SocketException>();
        }

        [TestMethod]
        public async Task ConnectCalledTwice_ThrowsException()
        {
            Exception thrownException = null;
            var underTest = new RedisClient();

            try
            {
                await underTest.Connect(LocalHostDefaultPort.AsConnectionSettings());
                await underTest.Connect(LocalHostDefaultPort.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            thrownException.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
        }

        [TestMethod]
        public async Task CallingClientAfterDispose_ThrowsException()
        {
            Exception thrownException = null;
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();
            underTest.Dispose();

            try
            {
                await underTest.Set(Key, Value);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            thrownException.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
        }

        [TestMethod]
        public async Task Select_ClientSelectsNewDb()
        {
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

            await underTest.Select(7);
            await underTest.Set(Key, Value);
            var result1 = await underTest.Get(Key);
            await underTest.Select(8);
            var result2 = await underTest.Get(Key);

            result1.Should().Be(Value);
            result2.Should().BeNull();
        }

        [TestMethod]
        public async Task SelectWrongDbNumber_ThrowsException()
        {
            Exception thrownException = null;
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

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
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

            await underTest.Set(Key, Value);
            var result1 = await underTest.Exists(Key);
            var result2 = await underTest.Exists("NotPresentKey");

            result1.Should().BeTrue();
            result2.Should().BeFalse();
        }

        [TestMethod]
        public async Task Del_DeletedSuccessfully()
        {
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

            await underTest.Set(Key, Value);
            var result1 = await underTest.Get(Key);
            await underTest.Del(Key);
            var result2 = await underTest.Get(Key);

            result1.Should().Be(Value);
            result2.Should().BeNull();
        }

        [TestMethod]
        public async Task FlushDb_DbFlushedSuccessfully()
        {
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

            await underTest.Set(Key, Value);
            await underTest.Set(Key2, Value2);
            await underTest.FlushDb();
            var result1 = await underTest.Get(Key);
            var result2 = await underTest.Get(Key2);

            result1.Should().BeNull();
            result2.Should().BeNull();
        }

        [TestMethod]
        public async Task FlushDbAsync_DbFlushedSuccessfully()
        {
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

            await underTest.Set(Key, Value);
            await underTest.Set(Key2, Value2);
            await underTest.FlushDb(true);
            Thread.Sleep(50);
            var result1 = await underTest.Get(Key);
            var result2 = await underTest.Get(Key2);

            result1.Should().BeNull();
            result2.Should().BeNull();
        }

        [TestMethod]
        public async Task DbSize_ReturnsValueCorrectly()
        {
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

            await underTest.Set(Key, Value);
            await underTest.Set(Key2, Value2);
            var result = await underTest.DbSize();

            result.Should().Be(2);
        }

        [TestMethod]
        public async Task SwapDb_ClientConnectedToCorrectDb()
        {
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

            await underTest.Select(0);
            await underTest.Set(Key, Value);
            await underTest.SwapDb(0, 7);
            var existsOnDb0 = await underTest.Exists(Key);
            await underTest.Select(7);
            var readValueFromDb7 = await underTest.Get(Key);

            existsOnDb0.Should().BeFalse();
            readValueFromDb7.Should().Be(Value);
        }

        [TestMethod]
        public async Task SwapWithWrongDbNumbers_ThrowsException()
        {
            Exception thrownException = null;
            var underTest = LocalHostDefaultPort.CreateAndConnectClient();

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


        [TestCleanup]
        public async Task Cleanup()
        {
            try
            {
                var dut = new RedisClient();
                await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

                await dut.Select(0);
                await dut.Del(Key);
                await dut.Del(Key2);

                await dut.Select(7);
                await dut.Del(Key);
                await dut.Del(Key2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}