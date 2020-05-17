using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestConfigurations;
using TestLite;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class HashTester
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

        [TestMethod]
        public void Test_SetAndGetHash() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest =>
            {
                underTest.HSet(_keys[0], Field1, Value1);
                return underTest.HGet(_keys[0], Field1);
            })
            .Assert().Validate(result => result.Should().Be(Value1));

        [TestMethod]
        public void Test_SetMultipleHash() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                var additionalFields = new Dictionary<string, string>
                {
                    {Field1, Value1},
                    {Field2, Value2},
                    {Field3, Value3}
                };
                return (client, additionalFields);
            })
            .Act((underTest, additionalFields) =>
            {
                underTest.HMSet(_keys[1], additionalFields);
                var res1 = underTest.HGet(_keys[1], Field1);
                var res2 = underTest.HGet(_keys[1], Field2);
                var res3 = underTest.HGet(_keys[1], Field3);
                return (res1, res2, res3);
            })
            .Assert()
                .Validate(result => result.res1.Should().Be(Value1))
                .Validate(result => result.res2.Should().Be(Value2))
                .Validate(result => result.res3.Should().Be(Value3));
        
        [TestMethod]
        public void Test_GetMultipleHash() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest =>
            {
                underTest.HSet(_keys[2], Field1, Value1);
                underTest.HSet(_keys[2], Field2, Value2);
                underTest.HSet(_keys[2], Field3, Value3);
                return underTest.HMGet(_keys[2], new[] { Field1, Field2, Field3 }).ToList();
            })
            .Assert()
                .Validate(result => result[0].Should().Be(Value1))
                .Validate(result => result[1].Should().Be(Value2))
                .Validate(result => result[2].Should().Be(Value3));

        [TestMethod]
        public void Test_GetAllHash() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest =>
            {
                underTest.HSet(_keys[3], Field1, Value1);
                underTest.HSet(_keys[3], Field2, Value2);
                underTest.HSet(_keys[3], Field3, Value3);
                return underTest.HGetAll(_keys[3]);
            })
            .Assert()
                .Validate(result => result.Count.Should().Be(3))
                .Validate(result => result[Field1].Should().Be(Value1))
                .Validate(result => result[Field2].Should().Be(Value2))
                .Validate(result => result[Field3].Should().Be(Value3));

        [TestMethod]
        public void Test_GetAllHash_Null() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest => underTest.HGetAll(_keys[3]))
            .Assert().Validate(result => result.Count.Should().Be(0));

        [TestMethod]
        public void Test_GetHash_Null() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest => underTest.HMGet(_keys[3], new[] { Field1, Field2 }).ToList())
            .Assert()
                .Validate(result => result.Count.Should().Be(2))
                .Validate(result => result[0].Should().BeNull())
                .Validate(result => result[1].Should().BeNull());


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