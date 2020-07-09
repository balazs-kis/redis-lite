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

            var dut = new AsyncRedisClient();
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
            var dut = new AsyncRedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Multi();
            await dut.Set(Key, Value1);
            await dut.Set(Key, Value2);
            await dut.Set(Key, Value3);
            await dut.Get(Key);
            var exec = await dut.Exec();

            var getResult = await dut.Get(Key);
            var execResult = exec.ToArray();

            Assert.AreEqual(Value3, getResult,
                "The result should be the same value as the input");

            Assert.AreEqual(RedisConstants.OkResult, execResult[0],
                "The 1st operation is SET, it should return a simple OK result");

            Assert.AreEqual(RedisConstants.OkResult, execResult[1],
                "The 2nd operation is SET, it should return a simple OK result");

            Assert.AreEqual(RedisConstants.OkResult, execResult[2],
                "The 3st operation is SET, it should return a simple OK result");

            Assert.AreEqual(Value3, execResult[3],
                "The 4th operation is GET, it should return the value that was in the latest SET");
        }

        [TestMethod]
        public async Task Test_TransactionExecuted_ListSetGet()
        {
            var items = new[] { "1", "20", "300", "4000" };

            var dut = new AsyncRedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Multi();
            await dut.RPush(Key, items);
            await dut.Del(Key);
            await dut.RPush(Key, items);
            var exec = await dut.Exec();

            var result = await dut.LRange(Key, 0, 4);
            var execResult = exec
                .Select(i =>
                {
                    if (i is string s)
                    {
                        return int.Parse(s);
                    }

                    return -1;
                })
                .ToArray();

            Assert.IsTrue(items.SequenceEqual(result),
                "The returned result should be the same array as the input");

            Assert.AreEqual(items.Length, execResult[0],
                "The 1st operation is RPUSH, it should return the number of items pushed into the list");

            Assert.AreEqual(1, execResult[1],
                "The 2nd operation is DEL, it should return the number of keys deleted");

            Assert.AreEqual(items.Length, execResult[2],
                "The 3rd operation is RPUSH, it should return the number of items pushed into the list");
        }

        [TestMethod]
        public async Task Test_TransactionFailed_SimpleSetGet()
        {
            Exception exception = null;

            var dut1 = new AsyncRedisClient();
            var dut2 = new AsyncRedisClient();

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

            var dut1 = new AsyncRedisClient();
            var dut2 = new AsyncRedisClient();

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
            var dut = new AsyncRedisClient();

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
            var dut = new AsyncRedisClient();

            await dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            await dut.Del(Key);
        }
    }
}