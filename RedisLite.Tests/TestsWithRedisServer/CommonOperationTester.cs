using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Client.Exceptions;
using RedisLite.Tests.TestConfigurations;
using System;
using System.Net.Sockets;

namespace RedisLite.Tests.TestsWithRedisServer
{
    [TestClass]
    public class CommonOperationTester
    {
        private const string Key = "TestKey";
        private const string Value = "TestValue";

        private const string Key2 = "TestKey2";
        private const string Value2 = "TestValue2";

        [TestMethod]
        public void Test_Connect()
        {
            Exception thrownException = null;

            var dut = new RedisClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNull(thrownException);
        }

        [TestMethod]
        public void TestUnknownHost_ConnectThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();

            try
            {
                dut.Connect(UnknownHost.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(SocketException));
        }

        [TestMethod]
        public void TestKnownHostBadPort_ConnectThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();

            try
            {
                dut.Connect(LocalHostPort7000.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(SocketException));
        }

        [TestMethod]
        public void TestConnectTwice_ConnectThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();

            try
            {
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TestCallingAfterDispose_ThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());
            dut.Dispose();

            try
            {
                dut.Set(Key, Value);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void Test_Select()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Select(7);
            dut.Set(Key, Value);
            var res1 = dut.Get(Key);

            dut.Select(8);
            var res2 = dut.Get(Key);

            Assert.AreEqual(Value, res1);
            Assert.IsNull(res2);
        }

        [TestMethod]
        public void TestWrongDbNumber_SelectThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.Select(int.MaxValue);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(RedisException));
        }

        [TestMethod]
        public void Test_Exists()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Set(Key, Value);
            var res1 = dut.Exists(Key);
            var res2 = dut.Exists("NotPresentKey");

            Assert.IsTrue(res1);
            Assert.IsFalse(res2);
        }

        [TestMethod]
        public void Test_Del()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Set(Key, Value);
            var res1 = dut.Get(Key);

            dut.Del(Key);
            var res2 = dut.Get(Key);

            Assert.AreEqual(Value, res1);
            Assert.IsNull(res2);
        }

        [TestMethod]
        public void Test_FlushDb()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Set(Key, Value);
            dut.Set(Key2, Value2);
            dut.FlushDb();
            var result1 = dut.Get(Key);
            var result2 = dut.Get(Key2);

            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void Test_FlushDbAsync()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Set(Key, Value);
            dut.Set(Key2, Value2);
            dut.FlushDb(true);
            var result1 = dut.Get(Key);
            var result2 = dut.Get(Key2);

            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void Test_DbSize()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Set(Key, Value);
            dut.Set(Key2, Value2);
            var result = dut.DbSize();

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void Test_SwapDb()
        {
            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            dut.Select(0);
            dut.Set(Key, Value);
            dut.SwapDb(0, 7);
            var existsOnDb0 = dut.Exists(Key);
            dut.Select(7);
            var result = dut.Get(Key);

            Assert.IsFalse(existsOnDb0);
            Assert.AreEqual(Value, result);
        }

        [TestMethod]
        public void TestWrongDbNumbers_SwapDbThrowsException()
        {
            Exception thrownException = null;

            var dut = new RedisClient();
            dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

            try
            {
                dut.SwapDb(int.MaxValue, int.MaxValue - 1);
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
            try
            {
                var dut = new RedisClient();
                dut.Connect(LocalHostDefaultPort.AsConnectionSettings());

                dut.Select(0);
                dut.Del(Key);
                dut.Del(Key2);

                dut.Select(7);
                dut.Del(Key);
                dut.Del(Key2);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during cleanup: {ex}");
                throw;
            }
        }
    }
}