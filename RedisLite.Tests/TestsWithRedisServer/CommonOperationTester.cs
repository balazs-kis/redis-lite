﻿using FluentAssertions;
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
            .Act(underTest => underTest.Connect(LocalHostDefaultPort.AsConnectionSettings()))
            .Assert().IsSuccess();

        [TestMethod]
        public void ConnectToUnknownHost_ThrowsException() => Test
            .Arrange(() => new RedisClient())
            .Act(underTest => underTest.Connect(UnknownHost.AsConnectionSettings()))
            .Assert().ThrewException<SocketException>();

        [TestMethod]
        public void ConnectToKnownHostWrongPort_ThrowsException() => Test
            .Arrange(() => new RedisClient())
            .Act(underTest => underTest.Connect(LocalHostPort7000.AsConnectionSettings()))
            .Assert().ThrewException<SocketException>();

        [TestMethod]
        public void ConnectCalledTwice_ThrowsException() => Test
            .Arrange(() => new RedisClient())
            .Act(underTest =>
            {
                underTest.Connect(LocalHostDefaultPort.AsConnectionSettings());
                underTest.Connect(LocalHostDefaultPort.AsConnectionSettings());
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
                underTest.Select(7);
                underTest.Set(Key, Value);
                var result1 = underTest.Get(Key);
                underTest.Select(8);
                var result2 = underTest.Get(Key);

                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().Be(Value))
                .Validate(result => result.result2.Should().BeNull());

        [TestMethod]
        public void SelectWrongDbNumber_ThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest => underTest.Select(int.MaxValue))
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Exists_ReturnsValueCorrectly() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(Key, Value);
                var result1 = underTest.Exists(Key);
                var result2 = underTest.Exists("NotPresentKey");

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
                underTest.Set(Key, Value);
                var result1 = underTest.Get(Key);
                underTest.Del(Key);
                var result2 = underTest.Get(Key);

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
                underTest.Set(Key, Value);
                underTest.Set(Key2, Value2);
                underTest.FlushDb();
                var result1 = underTest.Get(Key);
                var result2 = underTest.Get(Key2);

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
                underTest.Set(Key, Value);
                underTest.Set(Key2, Value2);
                underTest.FlushDb(true);
                Thread.Sleep(50);
                var result1 = underTest.Get(Key);
                var result2 = underTest.Get(Key2);

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
                underTest.Set(Key, Value);
                underTest.Set(Key2, Value2);

                return underTest.DbSize();
            })
            .Assert().Validate(result => result.Should().Be(2));

        [TestMethod]
        public void SwapDb_ClientConnectedToCorrectDb() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Select(0);
                underTest.Set(Key, Value);
                underTest.SwapDb(0, 7);
                var existsOnDb0 = underTest.Exists(Key);
                underTest.Select(7);
                var readValueFromDb7 = underTest.Get(Key);

                return (existsOnDb0, readValueFromDb7);
            })
            .Assert()
                .Validate(result => result.existsOnDb0.Should().BeFalse())
                .Validate(result => result.readValueFromDb7.Should().Be(Value));

        [TestMethod]
        public void SwapWithWrongDbNumbers_ThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest => underTest.SwapDb(int.MaxValue, int.MaxValue - 1))
            .Assert().ThrewException<RedisException>();

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var dut = new RedisClient();
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

                dut.Select(0);
                dut.Del(Key);
                dut.Del(Key2);

                dut.Select(7);
                dut.Del(Key);
                dut.Del(Key2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}