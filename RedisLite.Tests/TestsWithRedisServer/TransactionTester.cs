using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class TransactionTester
    {
        private const string Key = "TransactionKey";
        private const string Value1 = "TransactionValue1";
        private const string Value2 = "TransactionValue2";
        private const string Value3 = "TransactionValue3";

        [TestMethod]
        public void TestInvalidKeyList_WatchThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.Watch();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void Test_TransactionExecuted_SimpleSetGet()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Multi();
            dut.Set(Key, Value1);
            dut.Set(Key, Value2);
            dut.Set(Key, Value3);
            dut.Exec();

            var result = dut.Get(Key);

            Assert.AreEqual(Value3, result);
        }

        [TestMethod]
        public void Test_TransactionExecuted_ListSetGet()
        {
            var items = new[] { "1", "20", "300", "4000" };

            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Multi();
            dut.RPush(Key, items);
            dut.Del(Key);
            dut.RPush(Key, items);
            dut.Exec();

            var result = dut.LRange(Key, 0, 4);

            Assert.IsTrue(items.SequenceEqual(result));
        }

        [TestMethod]
        public void Test_TransactionFailed_SimpleSetGet()
        {
            Exception exception = null;

            var dut1 = new RedisClient();
            var dut2 = new RedisClient();

            dut1.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dut2.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut1.Watch(new[] { Key });

            dut1.Multi();
            dut1.Set(Key, Value1);

            dut2.Set(Key, Value2);

            dut1.Set(Key, Value2);
            dut1.Set(Key, Value3);

            try
            {
                dut1.Exec();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var result = dut1.Get(Key);

            Assert.AreEqual(Value2, result);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(RedisMultiExecutionFailedException));
        }

        [TestMethod]
        public void Test_TransactionFailed_ListSetGet()
        {
            var items = new[] { "1", "20", "300", "4000" };
            var five = "50000";
            var six = "600000";

            Exception exception = null;

            var dut1 = new RedisClient();
            var dut2 = new RedisClient();

            dut1.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dut2.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut1.RPush(Key, items);

            dut1.Watch(new[] { Key });

            dut1.Multi();
            dut1.RPush(Key, six);

            dut2.RPush(Key, five);

            try
            {
                dut1.Exec();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var result = dut1.LRange(Key, 4, 5);

            Assert.AreEqual(five, result.SingleOrDefault());
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(RedisMultiExecutionFailedException));
        }

        [TestMethod]
        public void Test_MultiDiscard()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Set(Key, Value1);

            dut.Multi();
            dut.Set(Key, Value2);
            dut.Set(Key, Value3);
            dut.Discard();

            var result = dut.Get(Key);

            Assert.AreEqual(Value1, result);
        }


        [TestCleanup]
        public void Cleanup()
        {
            var dut = new RedisClient();

            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Del(Key);
        }
    }
}