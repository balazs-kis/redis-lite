using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System;
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
        public void Test_SAdd()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.SAdd(SetKey, SetValue1);
            var result = dut.SMembers(SetKey).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(SetValue1, result.First());
        }

        [TestMethod]
        public void TestWrongOperation_SAddThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.Set(SetKey, SetValue1);
                dut.SAdd(SetKey, SetValue2);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public void Test_SRem()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.SAdd(SetKey, SetValue1);
            dut.SRem(SetKey, SetValue1);
            var result = dut.SMembers(SetKey).ToList();

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void TestWrongOperation_SRemThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.Set(SetKey, SetValue1);
                dut.SRem(SetKey, SetValue1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public void Test_SMembers()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.SAdd(SetKey, SetValue1, SetValue2);
            var result = dut.SMembers(SetKey).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains(SetValue1));
            Assert.IsTrue(result.Contains(SetValue2));
        }

        [TestMethod]
        public void TestWrongOperation_SMembersThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.Set(SetKey, SetValue1);
                dut.SMembers(SetKey);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public void Test_SIsMember()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.SAdd(SetKey, SetValue1);
            var result1 = dut.SIsMember(SetKey, SetValue1);
            var result2 = dut.SIsMember(SetKey, SetValue2);

            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void TestWrongOperation_SIsMemberThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.Set(SetKey, SetValue1);
                dut.SIsMember(SetKey, SetValue1);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }


        [TestCleanup]
        public void Cleanup()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Del(SetKey);
        }
    }
}