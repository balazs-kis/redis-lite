using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client.Networking;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RedisLite.Tests.TestHelpers;

namespace RedisLite.Tests.UnitTests
{
    [TestClass]
    public class LockerTests
    {
        private const int Number = 2020;

        [TestMethod]
        public void TryToRunOneAction_Succeeds() => Test
            .Arrange(() => new Locker())
            .Act(underTest =>
            {
                var result = 0;
                underTest.Execute(() => { result = Number; });

                return result;
            })
            .Assert(result => result.Should().Be(Number));

        [TestMethod]
        public void TryToRunSecondActionInParallel_LockerThrowsException() => Test
            .Arrange(() => (locker: new Locker(), are: new AutoResetEvent(false)))
            .Act(underTest =>
            {
                var resultNumber = 0;
                var exResult = Test.ForException(() =>
                {
                    Task.Run(() => underTest.locker.Execute(() => { underTest.are.WaitOne(); }));
                    Thread.Sleep(100);
                    underTest.locker.Execute(() => { resultNumber = Number; });
                });

                return (exResult, resultNumber);
            })
            .Assert(result =>
            {
                result.exResult.ThrewException.Should().BeTrue();
                result.exResult.Exception.Should().BeAssignableTo<InvalidOperationException>();
                result.resultNumber.Should().NotBe(Number);
            });

        [TestMethod]
        public void TryToRunOneFunc_Succeeds() => Test
            .Arrange(() => new Locker())
            .Act(underTest => underTest.Execute(() => Number))
            .Assert(result => result.Should().Be(Number));

        [TestMethod]
        public void TryToRunSecondFuncInParallel_LockerThrowsException() => Test
            .Arrange(() => (locker: new Locker(), are: new AutoResetEvent(false)))
            .Act(underTest =>
            {
                var resultNumber = 0;
                var task = Task.Run(() =>
                {
                    return underTest.locker.Execute(() =>
                    {
                        underTest.are.WaitOne();
                        return Number;
                    });
                });
                Thread.Sleep(100);
                var exResult = Test.ForException(() => resultNumber = underTest.locker.Execute(() => Number));
                underTest.are.Set();

                return (task, exResult, resultNumber);
            })
            .Assert(result =>
            {
                result.exResult.ThrewException.Should().BeTrue();
                result.exResult.Exception.Should().BeAssignableTo<InvalidOperationException>();
                result.resultNumber.Should().NotBe(Number);
                result.task.Result.Should().Be(Number);
            });

        [TestMethod]
        public void TryToObtainWhileNotLocked_Succeeds() => Test
            .Arrange(() => new Locker())
            .Act(underTest => Test.ForException(underTest.Obtain))
            .Assert(result => result.ThrewException.Should().BeFalse());

        [TestMethod]
        public void TryToObtainWhileLocked_LockerThrowsException() => Test
            .Arrange(() => (locker: new Locker(), are: new AutoResetEvent(false)))
            .Act(underTest =>
            {
                var (locker, are) = underTest;
                Task.Run(() => locker.Execute(() => { are.WaitOne(); }));
                Thread.Sleep(100);
                var exResult = Test.ForException(() => locker.Obtain());
                are.Set();

                return exResult;
            })
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<InvalidOperationException>();
            });

        [TestMethod]
        public void TryToObtainTwice_LockerThrowsException() => Test
            .Arrange(() => (locker: new Locker(), are: new AutoResetEvent(false)))
            .Act(underTest =>
            {
                Task.Run(() =>
                {
                    underTest.locker.Obtain();
                    underTest.are.WaitOne();
                    underTest.locker.Release();
                });
                Thread.Sleep(100);
                var result = Test.ForException(() => underTest.locker.Obtain());
                underTest.are.Set();

                return result;
            })
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<InvalidOperationException>();
            });

        [TestMethod]
        public void TryToObtainAfterReleased_Succeeds() => Test
            .Arrange(() => (locker: new Locker(), number1: 0, number2: 0))
            .Act(underTest =>
            {
                var (locker, number1, number2) = underTest;
                locker.Obtain();
                number1 = Number;
                locker.Release();
                var exResult = Task.Run(() =>
                {
                    return Test.ForException(() =>
                    {
                        locker.Obtain();
                        number2 = Number;
                        locker.Release();
                    });
                }).GetAwaiter().GetResult();

                return (exResult, number1, number2);
            })
            .Assert(result =>
            {
                result.exResult.ThrewException.Should().BeFalse();
                result.number1.Should().Be(Number);
                result.number2.Should().Be(Number);
            });
    }
}