using System;

namespace RedisLite.Tests.TestHelpers
{
    internal class ExecutionResult
    {
        public bool ThrewException { get; }
        public Exception Exception { get; }

        private ExecutionResult(bool threwException, Exception exception = null)
        {
            ThrewException = threwException;
            Exception = exception;
        }

        public static ExecutionResult CompletedWithoutException() =>
            new ExecutionResult(false);

        public static ExecutionResult CompletedWithException(Exception ex) =>
            new ExecutionResult(true, ex);
    }
}