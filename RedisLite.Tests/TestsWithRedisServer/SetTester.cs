using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.TestHelpers;
using RedisLite.Tests.TestConfigurations;
using System.Linq;

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
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1);
                return underTest.SMembers(SetKey).ToList();
            })
            .Assert()
                .Validate(result => result.Count.Should().Be(1))
                .Validate(result => result.First().Should().Be(SetValue1));

        [TestMethod]
        public void TestWrongOperation_SAddThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SAdd(SetKey, SetValue2);
            })
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Test_SRem() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1);
                underTest.SRem(SetKey, SetValue1);
                return underTest.SMembers(SetKey).ToList();
            })
            .Assert().Validate(result => result.Count.Should().Be(0));

        [TestMethod]
        public void TestWrongOperation_SRemThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SRem(SetKey, SetValue1);
            })
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Test_SMembers() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1, SetValue2);
                return underTest.SMembers(SetKey).ToList();
            })
            .Assert()
                .Validate(result => result.Count.Should().Be(2))
                .Validate(result => result.Should().Contain(SetValue1))
                .Validate(result => result.Should().Contain(SetValue2));

        [TestMethod]
        public void TestWrongOperation_SMembersThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SMembers(SetKey);
            })
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Test_SIsMember() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1);
                var result1 = underTest.SIsMember(SetKey, SetValue1);
                var result2 = underTest.SIsMember(SetKey, SetValue2);
                return (result1, result2);
            })
            .Assert()
                .Validate(result => result.result1.Should().BeTrue())
                .Validate(result => result.result2.Should().BeFalse());

        [TestMethod]
        public void TestWrongOperation_SIsMemberThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SIsMember(SetKey, SetValue1);
            })
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Test_SCard() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.SAdd(SetKey, SetValue1);
                underTest.SAdd(SetKey, SetValue2);
                return underTest.SCard(SetKey);
            })
            .Assert().Validate(result => result.Should().Be(2));

        [TestMethod]
        public void TestWrongOperation_SCardThrowsException() => Test
            .Arrange(LocalHostDefaultPort.CreateAndConnectClient)
            .Act(underTest =>
            {
                underTest.Set(SetKey, SetValue1);
                underTest.SCard(SetKey);
            })
            .Assert().ThrewException<RedisException>();


        [TestCleanup]
        public void Cleanup()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Del(SetKey);
        }
    }
}