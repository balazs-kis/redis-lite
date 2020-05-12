using System;

namespace RedisLite.TestHelpers
{
    public class ActResult
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Exception Exception { get; }

        protected ActResult(bool isSuccess, Exception exception = null)
        {
            IsSuccess = isSuccess;
            Exception = exception;
        }

        public static ActResult Ok() =>
            new ActResult(true);

        public static ActResult ThrewException(Exception ex) =>
            new ActResult(false, ex);

        public static ActResult<T> Ok<T>(T result) =>
            new ActResult<T>(true, null, result);

        public static ActResult<T> ThrewException<T>(Exception ex) =>
            new ActResult<T>(false, ex);
    }

    public sealed class ActResult<T> : ActResult
    {
        public T Value { get; }

        internal ActResult(bool threwException, Exception exception = null, T value = default)
            : base(threwException, exception)
        {
            Value = value;
        }
    }
}