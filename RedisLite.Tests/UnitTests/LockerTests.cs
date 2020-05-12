using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client.Networking;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RedisLite.TestHelpers;

namespace RedisLite.Tests.UnitTests
{
    [TestClass]
    public class LockerTests
    {
        private const int Number = 2020;

        private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(1250);

        [TestMethod]
        public void TryToRunOneAction_Succeeds() => Test
            .Arrange(() => new Locker())
            .Act(underTest =>
            {
                var result = 0;
                underTest.Execute(() => { result = Number; });

                return result;
            })
            .Assert(result => result.Value.Should().Be(Number));

        [TestMethod]
        public void TryToRunSecondActionInParallel_LockerThrowsException() => Test
            .Arrange(() =>
            {
                var locker = new Locker();
                var are = new AutoResetEvent(false);
                return (locker, are);
            })
            .Act((locker, are) =>
            {
                var resultNumber = 0;

                Task.Run(() => locker.Execute(() => { are.WaitOne(); }));
                Thread.Sleep(Delay);
                locker.Execute(() => { resultNumber = Number; });

                return resultNumber;
            })
            .Assert(result =>
            {
                result.IsFailure.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<InvalidOperationException>();
                result.Value.Should().NotBe(Number);
            });

        [TestMethod]
        public void TryToRunOneFunc_Succeeds() => Test
            .Arrange(() => new Locker())
            .Act(underTest => underTest.Execute(() => Number))
            .Assert(result => result.Value.Should().Be(Number));

        [TestMethod]
        public void TryToRunSecondFuncInParallel_LockerThrowsException() => Test
            .Arrange(() =>
            {
                var locker = new Locker();
                var are = new AutoResetEvent(false);
                return (locker, are);
            })
            .Act((locker, are) =>
            {
                Task.Run(() =>
                {
                    return locker.Execute(() =>
                    {
                        are.WaitOne();
                        return Number;
                    });
                });
                Thread.Sleep(Delay);
                var resultNumber = locker.Execute(() => Number);
                
                return resultNumber;
            })
            .Assert(result =>
            {
                result.IsFailure.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<InvalidOperationException>();
                result.Value.Should().NotBe(Number);
            });

        [TestMethod]
        public void TryToObtainWhileNotLocked_Succeeds() => Test
            .Arrange(() => new Locker())
            .Act(underTest => underTest.Obtain())
            .Assert(result => result.IsSuccess.Should().BeTrue());

        [TestMethod]
        public void TryToObtainWhileLocked_LockerThrowsException() => Test
            .Arrange(() =>
            {
                var locker = new Locker();
                var are = new AutoResetEvent(false);
                return (locker, are);
            })
            .Act((locker, are) =>
            {
                Task.Run(() => locker.Execute(() => { are.WaitOne(); }));
                Thread.Sleep(Delay);
                locker.Obtain();
            })
            .Assert(result =>
            {
                result.IsFailure.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<InvalidOperationException>();
            });

        [TestMethod]
        public void TryToObtainTwice_LockerThrowsException() => Test
            .Arrange(() =>
            {
                var locker = new Locker();
                var are = new AutoResetEvent(false);
                return (locker, are);
            })
            .Act((locker, are) =>
            {
                Task.Run(() =>
                {
                    locker.Obtain();
                    are.WaitOne();
                    locker.Release();
                });
                Thread.Sleep(Delay);
                locker.Obtain();
            })
            .Assert(result =>
            {
                result.IsFailure.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<InvalidOperationException>();
            });

        [TestMethod]
        public void TryToObtainAfterReleased_Succeeds() => Test
            .Arrange(() => new Locker())
            .Act(locker =>
            {
                int number1;
                var number2 = 0;

                locker.Obtain();
                number1 = Number;
                locker.Release();
                Task.Run(() =>
                {
                    locker.Obtain();
                    number2 = Number;
                    locker.Release();
                }).GetAwaiter().GetResult();

                return (number1, number2);
            })
            .Assert(result =>
            {
                result.IsSuccess.Should().BeTrue();
                result.Value.number1.Should().Be(Number);
                result.Value.number2.Should().Be(Number);
            });
    }
}