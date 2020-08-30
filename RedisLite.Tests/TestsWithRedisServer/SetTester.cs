using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System.Linq;
using System.Threading.Tasks;
using TestLite;

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
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.SAdd(SetKey, SetValue1);
                return (await underTest.SMembers(SetKey)).ToList();
            })
            .Assert()
                .Validate(result => result.Count.Should().Be(1))
                .Validate(result => result.First().Should().Be(SetValue1));

        [TestMethod]
        public void TestWrongOperation_SAddThrowsException() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.Set(SetKey, SetValue1);
                await underTest.SAdd(SetKey, SetValue2);
            })
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Test_SRem() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.SAdd(SetKey, SetValue1);
                await underTest.SRem(SetKey, SetValue1);
                return (await underTest.SMembers(SetKey)).ToList();
            })
            .Assert().Validate(result => result.Count.Should().Be(0));

        [TestMethod]
        public void TestWrongOperation_SRemThrowsException() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.Set(SetKey, SetValue1);
                await underTest.SRem(SetKey, SetValue1);
            })
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Test_SMembers() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.SAdd(SetKey, SetValue1, SetValue2);
                return (await underTest.SMembers(SetKey)).ToList();
            })
            .Assert()
                .Validate(result => result.Count.Should().Be(2))
                .Validate(result => result.Should().Contain(SetValue1))
                .Validate(result => result.Should().Contain(SetValue2));

        [TestMethod]
        public void TestWrongOperation_SMembersThrowsException() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.Set(SetKey, SetValue1);
                await underTest.SMembers(SetKey);
            })
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Test_SIsMember() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.SAdd(SetKey, SetValue1);
                var result1 = await underTest.SIsMember(SetKey, SetValue1);
                var result2 = await underTest.SIsMember(SetKey, SetValue2);
                return (result1, result2);
            })
            .Assert()
                .Validate(results => results.result1.Should().BeTrue())
                .Validate(results => results.result2.Should().BeFalse());

        [TestMethod]
        public void TestWrongOperation_SIsMemberThrowsException() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.Set(SetKey, SetValue1);
                await underTest.SIsMember(SetKey, SetValue1);
            })
            .Assert().ThrewException<RedisException>();

        [TestMethod]
        public void Test_SCard() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.SAdd(SetKey, SetValue1);
                await underTest.SAdd(SetKey, SetValue2);
                return await underTest.SCard(SetKey);
            })
            .Assert().Validate(result => result.Should().Be(2));

        [TestMethod]
        public void TestWrongOperation_SCardThrowsException() => Test
            .ArrangeAsync(LocalHostDefaultPort.CreateAndConnectClientAsync)
            .ActAsync(async underTest =>
            {
                await underTest.Set(SetKey, SetValue1);
                await underTest.SCard(SetKey);
            })
            .Assert().ThrewException<RedisException>();


        [TestCleanup]
        public async Task Cleanup()
        {
            var dut = new AsyncRedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Del(SetKey);
        }
    }
}