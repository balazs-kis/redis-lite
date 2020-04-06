using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisLite.Client;
using System;

namespace RedisLite.Tests.UnitTests
{
    [TestClass]
    public class ResultTests
    {
        [TestMethod]
        public void TestFailureWithoutMessage_ResultConstructionThrowsException()
        {
            Exception thrownException = null;

            try
            {
                Result.Fail(null);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TestFailureWithEmptyMessage_ResultConstructionThrowsException()
        {
            Exception thrownException = null;

            try
            {
                Result.Fail(string.Empty);
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOfType(thrownException, typeof(InvalidOperationException));
        }

        [TestMethod]
        public void TestOkResultToString_CorrectStringReturned()
        {
            var result = Result.Ok();
            var resultAsString = result.ToString();

            Assert.IsTrue(resultAsString.Contains("OK"));
        }

        [TestMethod]
        public void TestFailureResultToString_CorrectStringReturned()
        {
            const string errorMessage = "Message";
            const string exceptionMessage = "Exception message";
            var exceptionType = typeof(Exception);

            var result = Result.Fail(errorMessage, new Exception(exceptionMessage));
            var resultAsString = result.ToString();

            Assert.IsTrue(resultAsString.Contains("FAIL"));
            Assert.IsTrue(resultAsString.Contains(errorMessage));
            Assert.IsTrue(resultAsString.Contains(exceptionMessage));
            Assert.IsTrue(resultAsString.Contains(exceptionType.Name));
        }
    }
}