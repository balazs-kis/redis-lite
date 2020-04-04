using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client.Networking;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RedisLite.Tests.UnitTests
{
    [TestClass]
    public class LockerTests
    {
        private const int Number = 2020;

        [TestMethod]
        public void TryToRunOneAction_Succeeds()
        {
            var result = 0;
            var dut = new Locker();

            dut.Execute(() => { result = Number; });

            Assert.AreEqual(Number, result);
        }

        [TestMethod]
        public void TryToRunSecondActionInParallel_LockerThrowsException()
        {
            Exception thrownException = null;

            var are = new AutoResetEvent(false);
            var result = 0;
            var dut = new Locker();

            Task.Run(() => dut.Execute(() => { are.WaitOne(); }));
            Thread.Sleep(100);
            try
            {
                dut.Execute(() => { result = Number; });
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }
            are.Set();

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
            Assert.AreNotEqual(Number, result);
        }

        [TestMethod]
        public void TryToRunOneFunc_Succeeds()
        {
            var dut = new Locker();

            var result = dut.Execute(() => Number);

            Assert.AreEqual(Number, result);
        }

        [TestMethod]
        public void TryToRunSecondFuncInParallel_LockerThrowsException()
        {
            Exception thrownException = null;

            var are = new AutoResetEvent(false);
            var result = 0;
            var dut = new Locker();

            var task = Task.Run(() =>
            {
                return dut.Execute(() =>
                {
                    are.WaitOne();
                    return Number;
                });
            });
            Thread.Sleep(100);
            try
            {
                result = dut.Execute(() => Number);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }
            are.Set();

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
            Assert.AreNotEqual(Number, result);
            Assert.AreEqual(Number, task.Result);
        }

        [TestMethod]
        public void TryToObtainWhileNotLocked_Succeeds()
        {
            Exception thrownException = null;

            var dut = new Locker();

            try
            {
                dut.Obtain();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNull(thrownException);
        }

        [TestMethod]
        public void TryToObtainWhileLocked_LockerThrowsException()
        {
            Exception thrownException = null;

            var are = new AutoResetEvent(false);
            var dut = new Locker();

            Task.Run(() => dut.Execute(() => { are.WaitOne(); }));
            Thread.Sleep(100);
            try
            {
                dut.Obtain();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }
            are.Set();

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TryToObtainTwice_LockerThrowsException()
        {
            Exception thrownException = null;

            var are = new AutoResetEvent(false);
            var dut = new Locker();

            Task.Run(() =>
            {
                dut.Obtain();
                are.WaitOne();
                dut.Release();
            });
            Thread.Sleep(100);
            try
            {
                dut.Obtain();
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }
            are.Set();

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TryToObtainAfterReleased_Succeeds()
        {
            Exception thrownException = null;

            var result1 = 0;
            var result2 = 0;
            var dut = new Locker();

            dut.Obtain();
            result1 = Number;
            dut.Release();
            Task.Run(() =>
            {
                try
                {
                    dut.Obtain();
                    result2 = Number;
                    dut.Release();
                }
                catch (Exception ex)
                {
                    thrownException = ex;
                }
            }).GetAwaiter().GetResult();
            
            Assert.IsNull(thrownException);
            Assert.AreEqual(Number, result1);
            Assert.AreEqual(Number, result2);
        }
    }
}