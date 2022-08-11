using RedisLite.Client;

namespace RedisLite.UnitTests
{
    [TestClass]
    public class ResultTests
    {
        private const string ErrorMessage = "Message";
        private const string ExceptionMessage = "Exception message";

        [TestMethod]
        public void FailureResultConstructionWithoutMessage_ThrowsException() => Test
            .Arrange()
            .Act(() => Result.Fail(null))
            .Assert().ThrewException<InvalidOperationException>();

        [TestMethod]
        public void FailureResultConstructionWithEmptyMessage_ThrowsException() => Test
            .Arrange()
            .Act(() => Result.Fail(string.Empty))
            .Assert().ThrewException<InvalidOperationException>();

        [TestMethod]
        public void OkResultToString_CorrectStringReturned() => Test
            .Arrange()
            .Act(() => Result.Ok().ToString())
            .Assert().Validate(result => result.Should().Contain("OK"));

        [TestMethod]
        public void TestFailureResultToString_CorrectStringReturned() => Test
            .Arrange()
            .Act(() => Result.Fail(ErrorMessage, new AccessViolationException(ExceptionMessage)).ToString())
            .Assert()
                .Validate(result =>
                    result.Should()
                        .Contain("FAIL").And
                        .Contain(ErrorMessage).And
                        .Contain(ExceptionMessage).And
                        .Contain(nameof(AccessViolationException)));
    }
}