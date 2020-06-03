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
    public class TransactionTester
    {
        private const string Key = "TransactionKey";
        private const string Value1 = "TransactionValue1";
        private const string Value2 = "TransactionValue2";
        private const string Value3 = "TransactionValue3";

        [TestMethod]
        public async Task TestInvalidKeyList_WatchThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                await dut.Watch();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public async Task Test_TransactionExecuted_SimpleSetGet()
        {
            var dut = new RedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Multi();
            await dut.Set(Key, Value1);
            await dut.Set(Key, Value2);
            await dut.Set(Key, Value3);
            await dut.Exec();

            var result = dut.Get(Key);

            Assert.AreEqual(Value3, result);
        }

        [TestMethod]
        public async Task Test_TransactionExecuted_ListSetGet()
        {
            var items = new[] { "1", "20", "300", "4000" };

            var dut = new RedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Multi();
            await dut.RPush(Key, items);
            await dut.Del(Key);
            await dut.RPush(Key, items);
            await dut.Exec();

            var result = await dut.LRange(Key, 0, 4);

            Assert.IsTrue(items.SequenceEqual(result));
        }

        [TestMethod]
        public async Task Test_TransactionFailed_SimpleSetGet()
        {
            Exception exception = null;

            var dut1 = new RedisClient();
            var dut2 = new RedisClient();

            await dut1.Connect(LocalHostDefaultPort.AsConnectionSettings());
            await dut2.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut1.Watch(new[] { Key });

            await dut1.Multi();
            await dut1.Set(Key, Value1);

            await dut2.Set(Key, Value2);

            await dut1.Set(Key, Value2);
            await dut1.Set(Key, Value3);

            try
            {
                await dut1.Exec();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var result = await dut1.Get(Key);

            Assert.AreEqual(Value2, result);
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(RedisMultiExecutionFailedException));
        }

        [TestMethod]
        public async Task Test_TransactionFailed_ListSetGet()
        {
            var items = new[] { "1", "20", "300", "4000" };
            var five = "50000";
            var six = "600000";

            Exception exception = null;

            var dut1 = new RedisClient();
            var dut2 = new RedisClient();

            await dut1.Connect(LocalHostDefaultPort.AsConnectionSettings());
            await dut2.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut1.RPush(Key, items);

            await dut1.Watch(new[] { Key });

            await dut1.Multi();
            await dut1.RPush(Key, six);

            await dut2.RPush(Key, five);

            try
            {
                await dut1.Exec();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var result = await dut1.LRange(Key, 4, 5);

            Assert.AreEqual(five, result.SingleOrDefault());
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(RedisMultiExecutionFailedException));
        }

        [TestMethod]
        public async Task Test_MultiDiscard()
        {
            var dut = new RedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Set(Key, Value1);

            await dut.Multi();
            await dut.Set(Key, Value2);
            await dut.Set(Key, Value3);
            await dut.Discard();

            var result = await dut.Get(Key);

            Assert.AreEqual(Value1, result);
        }


        [TestCleanup]
        public async Task Cleanup()
        {
            var dut = new RedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Del(Key);
        }
    }
}