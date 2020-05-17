using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestConfigurations;
using TestLite;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class EventTester
    {
        private const string Key = "TestKey";
        private const string Value = "TestValue";

        private class StringWrapper
        {
            public string StringValue { get; set; }
        }

        [TestMethod]
        public void Test_ConnectedEvent() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                var result = new StringWrapper();

                client.OnConnected += c =>
                {
                    c.Set(Key, Value);
                    result.StringValue = client.Get(Key);
                };
                return (client, result);
            })
            .Act((client, result) =>
            {
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return result;
            })
            .Assert().Validate(result => result.StringValue.Should().Be(Value));
        
        
        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                var dut = new RedisClient();

                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

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