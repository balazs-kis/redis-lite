using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using RedisLite.Tests.TestHelpers;
using System;

namespace RedisLite.Tests.UnitTests
{
    [TestClass]
    public class ResultTests
    {
        private const string ErrorMessage = "Message";
        private const string ExceptionMessage = "Exception message";

        [TestMethod]
        public void FailureResultConstructionWithoutMessage_ThrowsException() => Test
            .ArrangeNotNeeded()
            .Act(() => Test.ForException(() => Result.Fail(null)))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<InvalidOperationException>();
            });

        [TestMethod]
        public void FailureResultConstructionWithEmptyMessage_ThrowsException() => Test
            .ArrangeNotNeeded()
            .Act(() => Test.ForException(() => Result.Fail(string.Empty)))
            .Assert(result =>
            {
                result.ThrewException.Should().BeTrue();
                result.Exception.Should().BeAssignableTo<InvalidOperationException>();
            });

        [TestMethod]
        public void OkResultToString_CorrectStringReturned() => Test
            .ArrangeNotNeeded()
            .Act(() => Result.Ok().ToString())
            .Assert(result => result.Should().Contain("OK"));

        [TestMethod]
        public void TestFailureResultToString_CorrectStringReturned() => Test
            .ArrangeNotNeeded()
            .Act(() => Result.Fail(ErrorMessage, new AccessViolationException(ExceptionMessage)).ToString())
            .Assert(result =>
            {
                result.Should().Contain("FAIL");
                result.Should().Contain(ErrorMessage);
                result.Should().Contain(ExceptionMessage);
                result.Should().Contain(typeof(AccessViolationException).Name);
            });
    }
}