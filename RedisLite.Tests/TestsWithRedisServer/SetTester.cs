using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class SetTester
    {
        private const string SetKey = "SetKey";
        private const string SetValue1 = "SetValue1";
        private const string SetValue2 = "SetValue2";

        [TestMethod]
        public async Task Test_SAdd()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.SAdd(SetKey, SetValue1);
            var result = (await dut.SMembers(SetKey)).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(SetValue1, result.First());
        }

        [TestMethod]
        public async Task TestWrongOperation_SAddThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                await dut.Set(SetKey, SetValue1);
                await dut.SAdd(SetKey, SetValue2);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public async Task Test_SRem()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.SAdd(SetKey, SetValue1);
            await dut.SRem(SetKey, SetValue1);
            var result = (await dut.SMembers(SetKey)).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task TestWrongOperation_SRemThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                await dut.Set(SetKey, SetValue1);
                await dut.SRem(SetKey, SetValue1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public async Task Test_SMembers()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.SAdd(SetKey, SetValue1, SetValue2);
            var result = (await dut.SMembers(SetKey)).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains(SetValue1));
            Assert.IsTrue(result.Contains(SetValue2));
        }

        [TestMethod]
        public async Task TestWrongOperation_SMembersThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                await dut.Set(SetKey, SetValue1);
                await dut.SMembers(SetKey);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public async Task Test_SIsMember()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.SAdd(SetKey, SetValue1);
            var result1 = await dut.SIsMember(SetKey, SetValue1);
            var result2 = await dut.SIsMember(SetKey, SetValue2);

            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public async Task TestWrongOperation_SIsMemberThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                await dut.Set(SetKey, SetValue1);
                await dut.SIsMember(SetKey, SetValue1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public async Task Test_SCard()
        {
            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.SAdd(SetKey, SetValue1);
            await dut.SAdd(SetKey, SetValue2);
            var result = await dut.SCard(SetKey);

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public async Task TestWrongOperation_SCardThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            long result = 0;

            try
            {
                await dut.Set(SetKey, SetValue1);
                result = await dut.SCard(SetKey);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.AreEqual(0, result);
            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }


        [TestCleanup]
        public async Task Cleanup()
        {
            var dut = new RedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Del(SetKey);
        }
    }
}