using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System.Linq;
using FluentAssertions;
using RedisLite.Tests.TestHelpers;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class SetTester
    {
        private const string SetKey = "SetKey";
        private const string SetValue1 = "SetValue1";
        private const string SetValue2 = "SetValue2";

        [TestMethod]
        public void Test_SAdd() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1);
                return underTest.SMembers(SetKey).ToList();
            })
            .Assert(result =>
            {
                result.Count.Should().Be(1);
                result.First().Should().Be(SetValue1);
            });

        [TestMethod]
        public void TestWrongOperation_SAddThrowsException() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest => Test.ForException(() =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SAdd(SetKey, SetValue2);
            }))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<RedisException>();
            });

        [TestMethod]
        public void Test_SRem() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1);
                underTest.SRem(SetKey, SetValue1);
                return underTest.SMembers(SetKey).ToList();
            })
            .Assert(result => result.Count.Should().Be(0));

        [TestMethod]
        public void TestWrongOperation_SRemThrowsException() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest => Test.ForException(() =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SRem(SetKey, SetValue1);
            }))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<RedisException>();
            });

        [TestMethod]
        public void Test_SMembers() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1, SetValue2);
                return underTest.SMembers(SetKey).ToList();
            })
            .Assert(result =>
            {
                result.Count.Should().Be(2);
                result.Should().Contain(SetValue1);
                result.Should().Contain(SetValue2);
            });

        [TestMethod]
        public void TestWrongOperation_SMembersThrowsException() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest => Test.ForException(() =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SMembers(SetKey);
            }))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<RedisException>();
            });
        
        [TestMethod]
        public void Test_SIsMember() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1);
                var result1 = underTest.SIsMember(SetKey, SetValue1);
                var result2 = underTest.SIsMember(SetKey, SetValue2);
                return (result1, result2);
            })
            .Assert(results =>
            {
                results.result1.Should().BeTrue();
                results.result2.Should().BeFalse();
            });

        [TestMethod]
        public void TestWrongOperation_SIsMemberThrowsException() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest => Test.ForException(() =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SIsMember(SetKey, SetValue1);
            }))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<RedisException>();
            });
        
        [TestMethod]
        public void Test_SCard() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1);
                underTest.SAdd(SetKey, SetValue2);
                return underTest.SCard(SetKey);
            })
            .Assert(result => result.Should().Be(2));

        [TestMethod]
        public void TestWrongOperation_SCardThrowsException() => Test
            .Arrange(() =>
            {
                var client = new RedisClient();
                client.Connect(LocalHostDefaultPort.AsConnectionSettings());
                return client;
            })
            .Act(underTest => Test.ForException(() =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SCard(SetKey);
            }))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<RedisException>();
            });


        [TestCleanup]
        public void Cleanup()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Del(SetKey);
        }
    }
}