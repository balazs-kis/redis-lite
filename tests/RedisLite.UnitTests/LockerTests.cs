using RedisLite.Client.Networking;

namespace RedisLite.UnitTests
{
    [TestClass]
    public class LockerTests
    {
        private const int Number = 2020;

        private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(1250);

        [TestMethod]
        public void TryToObtainWhileNotLocked_Succeeds() => Test
            .Arrange(() => new Locker())
            .Act(underTest => underTest.Obtain())
            .Assert().IsSuccess();

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
            .Assert().ThrewException<InvalidOperationException>();

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
            .Assert()
                .Validate(result => result.number1.Should().Be(Number))
                .Validate(result => result.number2.Should().Be(Number));

        [TestMethod]
        public void TryToReleaseWhenNotLocked_LockerThrowsException() => Test
            .Arrange(() => new Locker())
            .Act(locker => locker.Release())
            .Assert().ThrewException<InvalidOperationException>();
    }
}